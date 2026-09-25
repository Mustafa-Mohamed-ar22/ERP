using Microsoft.EntityFrameworkCore;

public class SalesOrderService : ISalesOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockService _stockService;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IAccountingIntegrationService _accountingIntegrationService;
    private readonly INotificationService _notificationService;
    public SalesOrderService(IUnitOfWork unitOfWork, IStockService stockService, ICurrentUserService currentUser, INumberSequenceService numberSequenceService, IAccountingIntegrationService accountingIntegrationService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _stockService = stockService;
        _currentUser = currentUser;
        _numberSequenceService = numberSequenceService;
        _accountingIntegrationService = accountingIntegrationService;
        _notificationService = notificationService;
    }

    public async Task<Result<List<SalesOrderResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = await _unitOfWork.SalesOrders.Query()
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);

        // Live stock warnings aren't computed for list results — would mean N extra queries per order
        // across a potentially large list. Call GetByIdAsync for a specific order's live availability.
        return Result.Success(orders.Select(o => ToResponse(o, new List<string>())).ToList());
    }

    public async Task<Result<SalesOrderResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await GetFullOrderAsync(id, ct);
        if (order is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotFound);

        var warnings = await ComputeStockWarningsAsync(order, ct);
        return Result.Success(ToResponse(order, warnings));
    }

    // CreateAsync, SubmitAsync, ApproveAsync, ShipGoodsAsync, CancelAsync are UNCHANGED —
    // they already end with `return await GetByIdAsync(order.Id, ct);`, so they automatically
    // pick up live warnings through the change above. No extra code needed in any of them.

    private async Task<List<string>> ComputeStockWarningsAsync(SalesOrder order, CancellationToken ct)
    {
        var warnings = new List<string>();

        if (order.Status is SalesOrderStatus.Shipped or SalesOrderStatus.Cancelled)
            return warnings; // nothing left to fulfill, nothing to warn about

        foreach (var line in order.Lines)
        {
            var remaining = line.Quantity - line.ShippedQuantity;
            if (remaining <= 0) continue;

            var available = await _unitOfWork.StockItems.Query()
                .Where(s => s.ProductId == line.ProductId && s.WarehouseId == order.WarehouseId)
                .Select(s => (decimal?)s.QuantityOnHand)
                .FirstOrDefaultAsync(ct) ?? 0;

            if (available < remaining)
            {
                warnings.Add($"{line.Product.Sku}: {remaining} remaining to fulfill, only {available} available in this warehouse");
            }
        }

        return warnings;
    }

    public async Task<Result<SalesOrderResponse>> CreateAsync(CreateSalesOrderRequest request, CancellationToken ct = default)
    {
        if (request.Lines is null || request.Lines.Count == 0)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.EmptyLines);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.CustomerNotFound);

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.WarehouseNotFound);

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var existingProductCount = await _unitOfWork.Products.Query().CountAsync(p => productIds.Contains(p.Id), ct);
        if (existingProductCount != productIds.Count)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.ProductNotFound);

        var orderNumber = await _numberSequenceService.GetNextNumberAsync("SalesOrder", "SO", 6, ct);
        var order = new SalesOrder
        {
            CompanyId = _currentUser.CompanyId,
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            OrderDate = request.OrderDate,
            Status = SalesOrderStatus.Draft,
            Notes = request.Notes,
            Lines = request.Lines.Select(l => new SalesOrderLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                ShippedQuantity = 0
            }).ToList()
        };

        await _unitOfWork.SalesOrders.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }
    public async Task<Result<SalesOrderResponse>> UpdateAsync(Guid id, CreateSalesOrderRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.SalesOrders.Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotFound);

        if (order.Status != SalesOrderStatus.Draft)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotDraft);

        if (request.Lines is null || request.Lines.Count == 0)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.EmptyLines);

        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.CustomerNotFound);

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.WarehouseNotFound);

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var existingProductCount = await _unitOfWork.Products.Query().CountAsync(p => productIds.Contains(p.Id), ct);
        if (existingProductCount != productIds.Count)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.ProductNotFound);

        foreach (var existingLine in order.Lines.ToList())
            _unitOfWork.SalesOrderLines.Remove(existingLine);

        order.CustomerId = request.CustomerId;
        order.WarehouseId = request.WarehouseId;
        order.OrderDate = request.OrderDate;
        order.Notes = request.Notes;

        foreach (var lineRequest in request.Lines)
        {
            await _unitOfWork.SalesOrderLines.AddAsync(new SalesOrderLine
            {
                SalesOrderId = order.Id,
                ProductId = lineRequest.ProductId,
                Quantity = lineRequest.Quantity,
                UnitPrice = lineRequest.UnitPrice,
                ShippedQuantity = 0
            }, ct);
        }
        _unitOfWork.SalesOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
        return await GetByIdAsync(order.Id, ct);
    }
    public async Task<Result<SalesOrderResponse>> SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.SalesOrders.GetByIdAsync(id, ct);
        if (order is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotFound);

        if (order.Status != SalesOrderStatus.Draft)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.InvalidStatusTransition);

        order.Status = SalesOrderStatus.Submitted;
        _unitOfWork.SalesOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<Result<SalesOrderResponse>> ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.SalesOrders.GetByIdAsync(id, ct);
        if (order is null) return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotFound);
        if (order.Status != SalesOrderStatus.Submitted) return Result.Failure<SalesOrderResponse>(SalesOrderErrors.InvalidStatusTransition);

        order.Status = SalesOrderStatus.Approved;
        _unitOfWork.SalesOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        if (order.CreatedBy is { } creatorUserId)
        {
            await _notificationService.NotifyUserAsync(creatorUserId,
                "تمت الموافقة على أمر البيع",
                $"تمت الموافقة على أمر البيع {order.OrderNumber}.",
                NotificationType.Success, null, ct);
        }

        return await GetByIdAsync(order.Id, ct);
    }

    //public async Task<Result<SalesOrderResponse>> ShipGoodsAsync(Guid id, ShipGoodsRequest request, CancellationToken ct = default)
    //{
    //    var order = await _unitOfWork.SalesOrders.Query()
    //        .Include(o => o.Lines)
    //            .ThenInclude(l => l.Product)
    //        .FirstOrDefaultAsync(o => o.Id == id, ct);

    //    if (order is null)
    //        return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotFound);

    //    if (order.Status is not (SalesOrderStatus.Approved or SalesOrderStatus.PartiallyShipped))
    //        return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotShippable);

    //    foreach (var shipLine in request.Lines)
    //    {
    //        var line = order.Lines.FirstOrDefault(l => l.Id == shipLine.LineId);
    //        if (line is null)
    //            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.LineNotFound);
    //        if (shipLine.QuantityShipped == 0)
    //            continue;
    //        var remaining = line.Quantity - line.ShippedQuantity;
    //        if (shipLine.QuantityShipped < 0 || shipLine.QuantityShipped > remaining)
    //            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.ShipQuantityExceedsRemaining);

    //        line.ShippedQuantity += shipLine.QuantityShipped;

    //        var movementResult = await _stockService.RecordMovementAsync(new RecordStockMovementRequest(
    //            line.ProductId, order.WarehouseId, StockMovementType.Out.ToString(), shipLine.QuantityShipped,
    //            $"Shipment against {order.OrderNumber}"), ct);

    //        if (!movementResult.IsSuccess)
    //            return Result.Failure<SalesOrderResponse>(movementResult.Error);
    //    }

    //    var shippedForPosting = request.Lines
    //        .Select(sl => order.Lines.First(l => l.Id == sl.LineId))
    //        .Select(l => (l.ProductId, request.Lines.First(sl => sl.LineId == l.Id).QuantityShipped, l.UnitPrice, l.Product.CostPrice))
    //        .ToList();

    //    var postingResult = await _accountingIntegrationService.PostShipmentAsync(order.OrderNumber, shippedForPosting, ct);
    //    if (!postingResult.IsSuccess)
    //        return Result.Failure<SalesOrderResponse>(postingResult.Error);

    //    var accountingNote = postingResult.Data == GLPostingOutcome.Skipped
    //        ? "Accounting integration not configured — this shipment was not posted to the General Ledger."
    //        : null;

    //    order.Status = order.Lines.All(l => l.ShippedQuantity >= l.Quantity)
    //        ? SalesOrderStatus.Shipped
    //        : SalesOrderStatus.PartiallyShipped;

    //    _unitOfWork.SalesOrders.Update(order);
    //    await _unitOfWork.SaveChangesAsync(ct);

    //    var response = await GetByIdAsync(order.Id, ct);

    //    if (accountingNote is not null && response.IsSuccess)
    //        response.Data.Warnings.Add(accountingNote);

    //    return response;
    //}
    public async Task<Result<SalesOrderResponse>> ShipGoodsAsync(Guid id, ShipGoodsRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.SalesOrders.Query()
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotFound);

        if (order.Status is not (SalesOrderStatus.Approved or SalesOrderStatus.PartiallyShipped))
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotShippable);

        // Validate every requested line before any database write happens.
        var linesToShip = new List<(SalesOrderLine Line, decimal QuantityShipped)>();
        foreach (var shipLine in request.Lines)
        {
            var line = order.Lines.FirstOrDefault(l => l.Id == shipLine.LineId);
            if (line is null)
                return Result.Failure<SalesOrderResponse>(SalesOrderErrors.LineNotFound);

            if (shipLine.QuantityShipped == 0)
                continue;

            var remaining = line.Quantity - line.ShippedQuantity;
            if (shipLine.QuantityShipped < 0 || shipLine.QuantityShipped > remaining)
                return Result.Failure<SalesOrderResponse>(SalesOrderErrors.ShipQuantityExceedsRemaining);

            linesToShip.Add((line, shipLine.QuantityShipped));
        }

        // SQL Server's retrying execution strategy must own the transaction boundary,
        // so BeginTransactionAsync/CommitAsync live inside ExecuteAsync's delegate.
        var strategy = _unitOfWork.CreateExecutionStrategy();

        var opResult = await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                foreach (var (line, quantityShipped) in linesToShip)
                {
                    line.ShippedQuantity += quantityShipped;

                    var movementResult = await _stockService.RecordMovementAsync(new RecordStockMovementRequest(
                        line.ProductId, order.WarehouseId, StockMovementType.Out.ToString(), quantityShipped,
                        $"Shipment against {order.OrderNumber}"), ct);

                    if (!movementResult.IsSuccess)
                    {
                        await tx.RollbackAsync(ct);
                        return Result.Failure<GLPostingOutcome>(movementResult.Error);
                    }
                }

                var shippedForPosting = linesToShip
                    .Select(x => (x.Line.ProductId, x.QuantityShipped, x.Line.UnitPrice, x.Line.Product.CostPrice))
                    .ToList();

                var postingResult = await _accountingIntegrationService.PostShipmentAsync(
                    order.OrderNumber, shippedForPosting, ct);

                if (!postingResult.IsSuccess)
                {
                    await tx.RollbackAsync(ct);
                    return Result.Failure<GLPostingOutcome>(postingResult.Error);
                }

                order.Status = order.Lines.All(l => l.ShippedQuantity >= l.Quantity)
                    ? SalesOrderStatus.Shipped
                    : SalesOrderStatus.PartiallyShipped;

                _unitOfWork.SalesOrders.Update(order);
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
            return Result.Failure<SalesOrderResponse>(opResult.Error);

        var accountingNote = opResult.Data == GLPostingOutcome.Skipped
            ? "Accounting integration not configured — this shipment was not posted to the General Ledger."
            : null;

        var response = await GetByIdAsync(order.Id, ct);

        if (accountingNote is not null && response.IsSuccess)
            response.Data.Warnings.Add(accountingNote);

        return response;
    }

    public async Task<Result<SalesOrderResponse>> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.SalesOrders.Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.NotFound);

        if (order.Status is SalesOrderStatus.Shipped or SalesOrderStatus.Cancelled)
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.InvalidStatusTransition);

        if (order.Lines.Any(l => l.ShippedQuantity > 0))
            return Result.Failure<SalesOrderResponse>(SalesOrderErrors.CannotCancel);

        order.Status = SalesOrderStatus.Cancelled;
        _unitOfWork.SalesOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }

    private async Task<SalesOrder?> GetFullOrderAsync(Guid id, CancellationToken ct) =>
        await _unitOfWork.SalesOrders.Query()
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    private static SalesOrderResponse ToResponse(SalesOrder o, List<string>? warnings = null) => new(
    o.Id, o.OrderNumber, o.CustomerId, o.Customer.Name, o.WarehouseId, o.Warehouse.Name, o.OrderDate,
    o.Status.ToString(), o.Notes, o.Lines.Sum(l => l.Quantity * l.UnitPrice),
    o.Lines.Select(l => new SalesOrderLineResponse(
        l.Id, l.ProductId, l.Product.Sku, l.Product.Name, l.Quantity, l.UnitPrice, l.ShippedQuantity)).ToList(),
    warnings ?? []);
}