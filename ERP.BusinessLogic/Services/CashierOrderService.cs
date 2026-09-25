using Microsoft.EntityFrameworkCore;

public class CashierOrderService : ICashierOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockService _stockService;
    private readonly IAccountingIntegrationService _accountingIntegrationService;
    private readonly IJournalEntryService _journalEntryService;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly ICurrentUserService _currentUser;

    public CashierOrderService(
        IUnitOfWork unitOfWork, IStockService stockService, IAccountingIntegrationService accountingIntegrationService,
        IJournalEntryService journalEntryService, INumberSequenceService numberSequenceService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _stockService = stockService;
        _accountingIntegrationService = accountingIntegrationService;
        _journalEntryService = journalEntryService;
        _numberSequenceService = numberSequenceService;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CashierOrderResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = await _unitOfWork.CashierOrders.Query()
            .Include(o => o.Customer).Include(o => o.Invoice)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.OrderDate).ToListAsync(ct);

        return Result.Success(orders.Select(o => ToResponse(o, new List<string>())).ToList());
    }

    public async Task<Result<CashierOrderResponse>> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await GetFullAsync(orderId, ct);
        return order is null
            ? Result.Failure<CashierOrderResponse>(CashierErrors.OrderNotFound)
            : Result.Success(ToResponse(order, new List<string>()));
    }

    public async Task<Result<CashierOrderResponse>> CreateOrderAsync(CreateCashierOrderRequest request, CancellationToken ct = default)
    {
        var lines = request.Lines ?? new List<CashierOrderLineRequest>();
        var payments = request.Payments ?? new List<CashierPaymentRequest>();

        if (lines.Count == 0)
            return Result.Failure<CashierOrderResponse>(CashierErrors.EmptyOrder);

        var shift = await _unitOfWork.CashierShifts.Query()
            .FirstOrDefaultAsync(s => s.CashierUserId == _currentUser.UserId && s.Status == CashierShiftStatus.Open, ct);
        if (shift is null)
            return Result.Failure<CashierOrderResponse>(CashierErrors.NoOpenShift);

        if (request.CustomerId is { } customerId)
        {
            var customerExists = await _unitOfWork.Customers.Query().AnyAsync(c => c.Id == customerId, ct);
            if (!customerExists) return Result.Failure<CashierOrderResponse>(CustomerErrors.NotFound);
        }

        var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _unitOfWork.Products.Query().Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);
        if (products.Count != productIds.Count)
            return Result.Failure<CashierOrderResponse>(CashierErrors.ProductNotFound);

        var parsedPayments = new List<(CashierPaymentMethod Method, decimal Amount, string? Reference)>();
        foreach (var p in payments)
        {
            if (!Enum.TryParse<CashierPaymentMethod>(p.Method, true, out var method))
                return Result.Failure<CashierOrderResponse>(CashierErrors.InvalidPaymentMethod);
            parsedPayments.Add((method, p.Amount, p.ReferenceNumber));
        }

        var subTotal = lines.Sum(l => l.Quantity * l.UnitPrice);
        var lineDiscounts = lines.Sum(l => l.DiscountAmount);
        var totalAmount = subTotal - lineDiscounts - request.DiscountAmount + request.TaxAmount;

        var cashTendered = parsedPayments.Where(p => p.Method == CashierPaymentMethod.Cash).Sum(p => p.Amount);
        var nonCashTendered = parsedPayments.Where(p => p.Method != CashierPaymentMethod.Cash).Sum(p => p.Amount);

        if (nonCashTendered > totalAmount)
            return Result.Failure<CashierOrderResponse>(CashierErrors.CardOverpayment);
        if (cashTendered + nonCashTendered < totalAmount)
            return Result.Failure<CashierOrderResponse>(CashierErrors.PaymentMismatch);

        var changeDue = (cashTendered + nonCashTendered) - totalAmount;

        var orderNumber = await _numberSequenceService.GetNextNumberAsync("CashierOrder", "POS", 6, ct);
        var invoiceNumber = await _numberSequenceService.GetNextNumberAsync("CashierInvoice", "CINV", 6, ct);

        var strategy = _unitOfWork.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                // Stock deducted per line — reuses IStockService's own InsufficientStock guard, no backorders for
                // a cash-register sale. Each call commits internally, hence the explicit outer transaction.
                foreach (var line in lines)
                {
                    var movementResult = await _stockService.RecordMovementAsync(new RecordStockMovementRequest(
                        line.ProductId, shift.WarehouseId, StockMovementType.Out.ToString(), line.Quantity,
                        $"Cashier sale: {orderNumber}"), ct);

                    if (!movementResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(ct);
                        return Result.Failure<CashierOrderResponse>(movementResult.Error);
                    }
                }

                var glLines = lines.Select(l => (l.ProductId, l.Quantity, l.UnitPrice, products[l.ProductId].CostPrice)).ToList();
                var postingResult = await _accountingIntegrationService.PostCashSaleAsync(orderNumber, glLines, ct);
                if (!postingResult.IsSuccess)
                {
                    await transaction.RollbackAsync(ct);
                    return Result.Failure<CashierOrderResponse>(postingResult.Error);
                }

                var order = new CashierOrder
                {
                    CompanyId = _currentUser.CompanyId,
                    OrderNumber = orderNumber,
                    CashierShiftId = shift.Id,
                    WarehouseId = shift.WarehouseId,
                    CustomerId = request.CustomerId,
                    WalkInCustomerName = request.WalkInCustomerName,
                    OrderDate = DateTime.UtcNow,
                    Status = CashierOrderStatus.Completed,
                    SubTotal = subTotal,
                    DiscountAmount = lineDiscounts + request.DiscountAmount,
                    TaxAmount = request.TaxAmount,
                    TotalAmount = totalAmount,
                    ChangeDue = changeDue,
                    JournalEntryId = postingResult.Data!.JournalEntryId,
                    WalkInCustomerPhone=request.WalkInCustomerPhone,
                    Lines = lines.Select(l => new CashierOrderLine
                    {
                        ProductId = l.ProductId,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        UnitCost = products[l.ProductId].CostPrice,
                        DiscountAmount = l.DiscountAmount,
                        LineTotal = (l.Quantity * l.UnitPrice) - l.DiscountAmount
                    }).ToList(),
                    Payments = parsedPayments.Select(p => new CashierPayment
                    {
                        Method = p.Method,
                        Amount = p.Amount,
                        ReferenceNumber = p.Reference
                    }).ToList()
                };

                var invoice = new CashierInvoice
                {
                    CompanyId = _currentUser.CompanyId,
                    InvoiceNumber = invoiceNumber,
                    CashierOrderId = order.Id,
                    CustomerId = request.CustomerId,
                    WalkInCustomerName = request.WalkInCustomerName,
                    InvoiceDate = DateTime.UtcNow,
                    Status = CashierInvoiceStatus.Issued,
                    SubTotal = order.SubTotal,
                    DiscountAmount = order.DiscountAmount,
                    TaxAmount = order.TaxAmount,
                    TotalAmount = order.TotalAmount,
                    AmountPaid = cashTendered + nonCashTendered,
                    ChangeDue = changeDue,
                    WalkInCustomerPhone=request.WalkInCustomerPhone,
                    Lines = lines.Select(l => new CashierInvoiceLine
                    {
                        ProductId = l.ProductId,
                        ProductNameSnapshot = products[l.ProductId].Name,
                        SkuSnapshot = products[l.ProductId].Sku,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountAmount = l.DiscountAmount,
                        LineTotal = (l.Quantity * l.UnitPrice) - l.DiscountAmount
                    }).ToList()
                };

                await _unitOfWork.CashierOrders.AddAsync(order, ct);
                await _unitOfWork.CashierInvoices.AddAsync(invoice, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                var warnings = postingResult.Data!.Outcome == GLPostingOutcome.Skipped
                    ? new List<string> { "Accounting integration not configured — this sale was not posted to the General Ledger." }
                    : new List<string>();

                var fullOrder = await GetFullAsync(order.Id, ct);
                return Result.Success(ToResponse(fullOrder!, warnings));
            }
            catch { await transaction.RollbackAsync(ct); throw; }
        });
    }

    public async Task<Result<CashierOrderResponse>> VoidOrderAsync(Guid orderId, VoidCashierOrderRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.CashierOrders.Query()
            .Include(o => o.Lines).Include(o => o.Invoice).Include(o => o.CashierShift)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null) return Result.Failure<CashierOrderResponse>(CashierErrors.OrderNotFound);
        if (order.Status == CashierOrderStatus.Voided) return Result.Failure<CashierOrderResponse>(CashierErrors.OrderAlreadyVoided);
        if (order.CashierShift.Status == CashierShiftStatus.Closed)
            return Result.Failure<CashierOrderResponse>(CashierErrors.CannotVoidAfterShiftClosed);

        var strategy = _unitOfWork.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                foreach (var line in order.Lines)
                {
                    var restoreResult = await _stockService.RecordMovementAsync(new RecordStockMovementRequest(
                        line.ProductId, order.WarehouseId, StockMovementType.AdjustmentIncrease.ToString(), line.Quantity,
                        $"Void: {order.OrderNumber}"), ct);

                    if (!restoreResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(ct);
                        return Result.Failure<CashierOrderResponse>(restoreResult.Error);
                    }
                }

                if (order.JournalEntryId is { } journalEntryId)
                {
                    var reverseResult = await _journalEntryService.ReverseAsync(journalEntryId, ct);
                    if (!reverseResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(ct);
                        return Result.Failure<CashierOrderResponse>(reverseResult.Error);
                    }
                }

                order.Status = CashierOrderStatus.Voided;
                order.VoidedAt = DateTime.UtcNow;
                order.VoidedByUserId = _currentUser.UserId;
                order.VoidReason = request.Reason;

                if (order.Invoice is not null)
                    order.Invoice.Status = CashierInvoiceStatus.Voided;

                _unitOfWork.CashierOrders.Update(order);
                await _unitOfWork.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                var fullOrder = await GetFullAsync(order.Id, ct);
                return Result.Success(ToResponse(fullOrder!, new List<string>()));
            }
            catch { await transaction.RollbackAsync(ct); throw; }
        });
    }
    public async Task<Result<List<CashierInvoiceResponse>>> GetMyInvoicesAsync(CancellationToken ct = default)
        => await GetInvoicesByUserIdAsync(_currentUser.UserId, ct);

    public async Task<Result<List<CashierInvoiceResponse>>> GetInvoicesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        // "Cashier" here means whoever's shift the order was created under — CashierOrder doesn't store the
        // cashier directly, it's reached via CashierShift.CashierUserId.
        var invoices = await _unitOfWork.CashierInvoices.Query()
            .Include(i => i.Lines)
            .Include(i => i.CashierOrder).ThenInclude(o => o.CashierShift)
            .Where(i => i.CashierOrder.CashierShift.CashierUserId == userId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(ct);

        return Result.Success(invoices.Select(ToInvoiceResponse).ToList());
    }
    public async Task<Result<CashierInvoiceResponse>> GetInvoiceByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.CashierInvoices.Query()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.CashierOrderId == orderId, ct);

        return invoice is null
            ? Result.Failure<CashierInvoiceResponse>(CashierErrors.InvoiceNotFound)
            : Result.Success(ToInvoiceResponse(invoice));
    }

    public async Task<Result<CashierInvoiceResponse>> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.CashierInvoices.Query()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct);

        return invoice is null
            ? Result.Failure<CashierInvoiceResponse>(CashierErrors.InvoiceNotFound)
            : Result.Success(ToInvoiceResponse(invoice));
    }

    private async Task<CashierOrder?> GetFullAsync(Guid id, CancellationToken ct) =>
        await _unitOfWork.CashierOrders.Query()
            .Include(o => o.Customer).Include(o => o.Invoice)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    private static CashierOrderResponse ToResponse(CashierOrder o, List<string> warnings) => new(
        o.Id, o.OrderNumber, o.CashierShiftId, o.WarehouseId, o.CustomerId, o.WalkInCustomerName, o.WalkInCustomerPhone, o.OrderDate,
        o.Status.ToString(), o.SubTotal, o.DiscountAmount, o.TaxAmount, o.TotalAmount, o.ChangeDue,
        o.Invoice?.Id, o.Invoice?.InvoiceNumber,
        o.Lines.Select(l => new CashierOrderLineResponse(l.Id, l.ProductId, l.Product.Sku, l.Product.Name, l.Quantity, l.UnitPrice, l.DiscountAmount, l.LineTotal)).ToList(),
        o.Payments.Select(p => new CashierPaymentResponse(p.Id, p.Method.ToString(), p.Amount, p.ReferenceNumber)).ToList(),
        warnings);

    private static CashierInvoiceResponse ToInvoiceResponse(CashierInvoice i) => new(
        i.Id, i.InvoiceNumber, i.CashierOrderId, i.CustomerId, i.WalkInCustomerName, i.WalkInCustomerPhone, i.InvoiceDate, i.Status.ToString(),
        i.SubTotal, i.DiscountAmount, i.TaxAmount, i.TotalAmount, i.AmountPaid, i.ChangeDue,
        i.Lines.Select(l => new CashierInvoiceLineResponse(l.ProductId, l.ProductNameSnapshot, l.SkuSnapshot, l.Quantity, l.UnitPrice, l.DiscountAmount, l.LineTotal)).ToList());

}