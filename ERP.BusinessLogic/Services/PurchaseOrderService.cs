using Microsoft.EntityFrameworkCore;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockService _stockService;   // ← the actual cross-module link to Inventory
    private readonly ICurrentUserService _currentUser;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IAccountingIntegrationService _accountingIntegrationService;

    public PurchaseOrderService(IUnitOfWork unitOfWork, IStockService stockService, ICurrentUserService currentUser, INumberSequenceService numberSequenceService, IAccountingIntegrationService accountingIntegrationService)
    {
        _unitOfWork = unitOfWork;
        _stockService = stockService;
        _currentUser = currentUser;
        _numberSequenceService = numberSequenceService;
        _accountingIntegrationService = accountingIntegrationService;
    }
    public async Task<Result<List<PurchaseOrderResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = await _unitOfWork.PurchaseOrders.Query()
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);

        return Result.Success(orders.Select(o => ToResponse(o)).ToList());
    }
    public async Task<Result<PurchaseOrderResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await GetFullOrderAsync(id, ct);
        if (order is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        return Result.Success(ToResponse(order));
    }

    public async Task<Result<PurchaseOrderResponse>> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.EmptyLines);

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, ct);
        if (supplier is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.SupplierNotFound);

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.WarehouseNotFound);

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var existingProductCount = await _unitOfWork.Products.Query().CountAsync(p => productIds.Contains(p.Id), ct);
        if (existingProductCount != productIds.Count)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.ProductNotFound);

        var orderNumber = await _numberSequenceService.GetNextNumberAsync("PurchaseOrder", "PO", 6, ct);
        // Same sequential-numbering race condition noted in the Accounting module (EntryNumber) applies here —
        // acceptable at current scale, revisit with a proper counter/sequence if concurrent order creation becomes real.

        var order = new PurchaseOrder
        {
            CompanyId = _currentUser.CompanyId,
            OrderNumber = orderNumber,
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            OrderDate = request.OrderDate,
            Status = PurchaseOrderStatus.Draft,
            Notes = request.Notes,
            Lines = request.Lines.Select(l => new PurchaseOrderLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                ReceivedQuantity = 0
            }).ToList()
        };

        await _unitOfWork.PurchaseOrders.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }
    public async Task<Result<PurchaseOrderResponse>> UpdateAsync(Guid id, CreatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        if (order.Status != PurchaseOrderStatus.Draft)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotDraft);

        if (request.Lines is null || request.Lines.Count == 0)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.EmptyLines);

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.SupplierId, ct);
        if (supplier is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.SupplierNotFound);

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.WarehouseNotFound);

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var existingProductCount = await _unitOfWork.Products.Query().CountAsync(p => productIds.Contains(p.Id), ct);
        if (existingProductCount != productIds.Count)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.ProductNotFound);

        foreach (var existingLine in order.Lines.ToList())
            _unitOfWork.PurchaseOrderLines.Remove(existingLine);

        order.SupplierId = request.SupplierId;
        order.WarehouseId = request.WarehouseId;
        order.OrderDate = request.OrderDate;
        order.Notes = request.Notes;

        foreach (var lineRequest in request.Lines)
        {
            await _unitOfWork.PurchaseOrderLines.AddAsync(new PurchaseOrderLine
            {
                PurchaseOrderId = order.Id,
                ProductId = lineRequest.ProductId,
                Quantity = lineRequest.Quantity,
                UnitPrice = lineRequest.UnitPrice,
                ReceivedQuantity = 0
            }, ct);
        }

        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }
    public async Task<Result<PurchaseOrderResponse>> SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdAsync(id, ct);
        if (order is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        if (order.Status != PurchaseOrderStatus.Draft)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.InvalidStatusTransition);

        order.Status = PurchaseOrderStatus.Submitted;
        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<Result<PurchaseOrderResponse>> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdAsync(id, ct);
        if (order is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        if (order.Status != PurchaseOrderStatus.Submitted)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.InvalidStatusTransition);

        order.Status = PurchaseOrderStatus.Approved;
        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }

    //public async Task<Result<PurchaseOrderResponse>> ReceiveGoodsAsync(Guid id, ReceiveGoodsRequest request, CancellationToken ct = default)
    //{
    //    var order = await _unitOfWork.PurchaseOrders.Query()
    //        .Include(o => o.Lines)
    //        .FirstOrDefaultAsync(o => o.Id == id, ct);

    //    if (order is null)
    //        return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

    //    if (order.Status is not (PurchaseOrderStatus.Approved or PurchaseOrderStatus.PartiallyReceived))
    //        return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotReceivable);

    //    foreach (var receiveLine in request.Lines)
    //    {
    //        var line = order.Lines.FirstOrDefault(l => l.Id == receiveLine.LineId);
    //        if (line is null)
    //            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.LineNotFound);
    //        if (receiveLine.QuantityReceived == 0)
    //            continue;
    //        var remaining = line.Quantity - line.ReceivedQuantity;
    //        if (receiveLine.QuantityReceived < 0 || receiveLine.QuantityReceived > remaining)
    //            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.ReceiveQuantityExceedsRemaining);

    //        line.ReceivedQuantity += receiveLine.QuantityReceived;

    //        var movementResult = await _stockService.RecordMovementAsync(new RecordStockMovementRequest(
    //            line.ProductId, order.WarehouseId, StockMovementType.In.ToString(), receiveLine.QuantityReceived,
    //            $"Receipt against {order.OrderNumber}"), ct);

    //        if (!movementResult.IsSuccess)
    //            return Result.Failure<PurchaseOrderResponse>(movementResult.Error);
    //    }

    //    var receivedForPosting = request.Lines
    //        .Select(rl => order.Lines.First(l => l.Id == rl.LineId))
    //        .Select(l => (l.ProductId, request.Lines.First(rl => rl.LineId == l.Id).QuantityReceived, l.UnitPrice))
    //        .ToList();

    //    var postingResult = await _accountingIntegrationService.PostGoodsReceiptAsync(order.OrderNumber, receivedForPosting, ct);
    //    if (!postingResult.IsSuccess)
    //        return Result.Failure<PurchaseOrderResponse>(postingResult.Error);

    //    var accountingNote = postingResult.Data == GLPostingOutcome.Skipped
    //        ? "Accounting integration not configured — this receipt was not posted to the General Ledger."
    //        : null;

    //    order.Status = order.Lines.All(l => l.ReceivedQuantity >= l.Quantity)
    //        ? PurchaseOrderStatus.Received
    //        : PurchaseOrderStatus.PartiallyReceived;

    //    _unitOfWork.PurchaseOrders.Update(order);
    //    await _unitOfWork.SaveChangesAsync(ct);

    //    var response = await GetByIdAsync(order.Id, ct);

    //    if (accountingNote is not null && response.IsSuccess)
    //        response.Data.Warnings.Add(accountingNote);

    //    return response;
    //}
    public async Task<Result<PurchaseOrderResponse>> ReceiveGoodsAsync(Guid id, ReceiveGoodsRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        if (order.Status is not (PurchaseOrderStatus.Approved or PurchaseOrderStatus.PartiallyReceived))
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotReceivable);

        // Validate every requested line before any database write happens.
        var linesToReceive = new List<(PurchaseOrderLine Line, decimal QuantityReceived)>();
        foreach (var receiveLine in request.Lines)
        {
            var line = order.Lines.FirstOrDefault(l => l.Id == receiveLine.LineId);
            if (line is null)
                return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.LineNotFound);

            if (receiveLine.QuantityReceived == 0)
                continue;

            var remaining = line.Quantity - line.ReceivedQuantity;
            if (receiveLine.QuantityReceived < 0 || receiveLine.QuantityReceived > remaining)
                return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.ReceiveQuantityExceedsRemaining);

            linesToReceive.Add((line, receiveLine.QuantityReceived));
        }

        var strategy = _unitOfWork.CreateExecutionStrategy();

        var opResult = await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                foreach (var (line, quantityReceived) in linesToReceive)
                {
                    line.ReceivedQuantity += quantityReceived;

                    var movementResult = await _stockService.RecordMovementAsync(new RecordStockMovementRequest(
                        line.ProductId, order.WarehouseId, StockMovementType.In.ToString(), quantityReceived,
                        $"Receipt against {order.OrderNumber}"), ct);

                    if (!movementResult.IsSuccess)
                    {
                        await tx.RollbackAsync(ct);
                        return Result.Failure<GLPostingOutcome>(movementResult.Error);
                    }
                }

                var receivedForPosting = linesToReceive
                    .Select(x => (x.Line.ProductId, x.QuantityReceived, x.Line.UnitPrice))
                    .ToList();

                var postingResult = await _accountingIntegrationService.PostGoodsReceiptAsync(
                    order.OrderNumber, receivedForPosting, ct);

                if (!postingResult.IsSuccess)
                {
                    await tx.RollbackAsync(ct);
                    return Result.Failure<GLPostingOutcome>(postingResult.Error);
                }

                order.Status = order.Lines.All(l => l.ReceivedQuantity >= l.Quantity)
                    ? PurchaseOrderStatus.Received
                    : PurchaseOrderStatus.PartiallyReceived;

                _unitOfWork.PurchaseOrders.Update(order);
                await _unitOfWork.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                return Result.Success(postingResult.Data);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });

        if (!opResult.IsSuccess)
            return Result.Failure<PurchaseOrderResponse>(opResult.Error);

        var accountingNote = opResult.Data == GLPostingOutcome.Skipped
            ? "Accounting integration not configured — this receipt was not posted to the General Ledger."
            : null;

        var response = await GetByIdAsync(order.Id, ct);

        if (accountingNote is not null && response.IsSuccess)
            response.Data.Warnings.Add(accountingNote);

        return response;
    }
    public async Task<Result<PurchaseOrderResponse>> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.NotFound);

        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.InvalidStatusTransition);

        if (order.Lines.Any(l => l.ReceivedQuantity > 0))
            return Result.Failure<PurchaseOrderResponse>(PurchaseOrderErrors.CannotCancel);

        order.Status = PurchaseOrderStatus.Cancelled;
        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }

    private async Task<PurchaseOrder?> GetFullOrderAsync(Guid id, CancellationToken ct) =>
        await _unitOfWork.PurchaseOrders.Query()
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    private static PurchaseOrderResponse ToResponse(PurchaseOrder o, List<string>? warnings = null) => new(
    o.Id, o.OrderNumber, o.SupplierId, o.Supplier.Name, o.WarehouseId, o.Warehouse.Name, o.OrderDate,
    o.Status.ToString(), o.Notes, o.Lines.Sum(l => l.Quantity * l.UnitPrice),
    o.Lines.Select(l => new PurchaseOrderLineResponse(
        l.Id, l.ProductId, l.Product.Sku, l.Product.Name, l.Quantity, l.UnitPrice, l.ReceivedQuantity)).ToList(),
    warnings ?? []);
}