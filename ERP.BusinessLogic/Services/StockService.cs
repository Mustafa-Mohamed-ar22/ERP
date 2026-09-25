using Microsoft.EntityFrameworkCore;
public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public StockService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<StockLevelResponse>>> GetProductStockAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct);
        if (product is null)
            return Result.Failure<List<StockLevelResponse>>(StockErrors.ProductNotFound);

        var stockItems = await _unitOfWork.StockItems.Query()
            .Where(s => s.ProductId == productId)
            .Include(s => s.Warehouse)
            .ToListAsync(ct);

        return Result.Success(stockItems.Select(s => new StockLevelResponse(
            product.Id, product.Sku, product.Name, product.SalePrice, s.WarehouseId, s.Warehouse.Name, s.QuantityOnHand)).ToList());
    }

    public async Task<Result<List<StockLevelResponse>>> GetWarehouseStockAsync(Guid warehouseId, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId, ct);
        if (warehouse is null)
            return Result.Failure<List<StockLevelResponse>>(StockErrors.WarehouseNotFound);

        var stockItems = await _unitOfWork.StockItems.Query()
            .Where(s => s.WarehouseId == warehouseId)
            .Include(s => s.Product)
            .ToListAsync(ct);

        return Result.Success(stockItems.Select(s => new StockLevelResponse(
            s.ProductId, s.Product.Sku, s.Product.Name, s.Product.SalePrice, warehouse.Id, warehouse.Name, s.QuantityOnHand)).ToList());
    }

    public async Task<Result<StockMovementResponse>> RecordMovementAsync(RecordStockMovementRequest request, CancellationToken ct = default)
    {
        // TransferIn/TransferOut are only ever created internally by TransferAsync — reject them here
        if (!Enum.TryParse<StockMovementType>(request.MovementType, true, out var movementType)
            || movementType is StockMovementType.TransferIn or StockMovementType.TransferOut)
        {
            return Result.Failure<StockMovementResponse>(StockErrors.InvalidMovementType);
        }

        if (request.Quantity <= 0)
            return Result.Failure<StockMovementResponse>(StockErrors.InvalidQuantity);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.Failure<StockMovementResponse>(StockErrors.ProductNotFound);

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse is null)
            return Result.Failure<StockMovementResponse>(StockErrors.WarehouseNotFound);

        var stockItem = await _unitOfWork.StockItems.Query()
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId, ct);

        var isIncrease = movementType is StockMovementType.In or StockMovementType.AdjustmentIncrease;

        if (stockItem is null)
        {
            if (!isIncrease)
                return Result.Failure<StockMovementResponse>(StockErrors.InsufficientStock);

            stockItem = new StockItem
            {
                CompanyId = _currentUser.CompanyId,
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                QuantityOnHand = 0
            };
            await _unitOfWork.StockItems.AddAsync(stockItem, ct);
        }

        if (isIncrease)
        {
            stockItem.QuantityOnHand += request.Quantity;
        }
        else
        {
            if (stockItem.QuantityOnHand < request.Quantity)
                return Result.Failure<StockMovementResponse>(StockErrors.InsufficientStock);

            stockItem.QuantityOnHand -= request.Quantity;
        }

        var movement = new StockMovement
        {
            CompanyId = _currentUser.CompanyId,
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            MovementType = movementType,
            Quantity = request.Quantity,
            Reference = request.Reference,
            MovementDate = DateTime.UtcNow
        };

        await _unitOfWork.StockMovements.AddAsync(movement, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<StockMovementResponse>(StockErrors.ConcurrencyConflict);
        }

        return Result.Success(ToResponse(movement, product, warehouse));
    }

    public async Task<Result<List<StockMovementResponse>>> TransferAsync(TransferStockRequest request, CancellationToken ct = default)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            return Result.Failure<List<StockMovementResponse>>(StockErrors.SameWarehouseTransfer);

        if (request.Quantity <= 0)
            return Result.Failure<List<StockMovementResponse>>(StockErrors.InvalidQuantity);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.Failure<List<StockMovementResponse>>(StockErrors.ProductNotFound);

        var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.FromWarehouseId, ct);
        var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.ToWarehouseId, ct);
        if (fromWarehouse is null || toWarehouse is null)
            return Result.Failure<List<StockMovementResponse>>(StockErrors.WarehouseNotFound);

        var fromStockItem = await _unitOfWork.StockItems.Query()
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.FromWarehouseId, ct);

        if (fromStockItem is null || fromStockItem.QuantityOnHand < request.Quantity)
            return Result.Failure<List<StockMovementResponse>>(StockErrors.InsufficientStock);

        fromStockItem.QuantityOnHand -= request.Quantity;

        var toStockItem = await _unitOfWork.StockItems.Query()
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.ToWarehouseId, ct);

        if (toStockItem is null)
        {
            toStockItem = new StockItem
            {
                CompanyId = _currentUser.CompanyId,
                ProductId = request.ProductId,
                WarehouseId = request.ToWarehouseId,
                QuantityOnHand = 0
            };
            await _unitOfWork.StockItems.AddAsync(toStockItem, ct);
        }
        toStockItem.QuantityOnHand += request.Quantity;

        var outMovement = new StockMovement
        {
            CompanyId = _currentUser.CompanyId,
            ProductId = request.ProductId,
            WarehouseId = request.FromWarehouseId,
            RelatedWarehouseId = request.ToWarehouseId,
            MovementType = StockMovementType.TransferOut,
            Quantity = request.Quantity,
            Reference = request.Reference,
            MovementDate = DateTime.UtcNow
        };

        var inMovement = new StockMovement
        {
            CompanyId = _currentUser.CompanyId,
            ProductId = request.ProductId,
            WarehouseId = request.ToWarehouseId,
            RelatedWarehouseId = request.FromWarehouseId,
            MovementType = StockMovementType.TransferIn,
            Quantity = request.Quantity,
            Reference = request.Reference,
            MovementDate = DateTime.UtcNow
        };

        await _unitOfWork.StockMovements.AddAsync(outMovement, ct);
        await _unitOfWork.StockMovements.AddAsync(inMovement, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct); // both StockItem updates + both movements commit together, atomically
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<List<StockMovementResponse>>(StockErrors.ConcurrencyConflict);
        }

        return Result.Success(new List<StockMovementResponse>
        {
            ToResponse(outMovement, product, fromWarehouse),
            ToResponse(inMovement, product, toWarehouse)
        });
    }

    private static StockMovementResponse ToResponse(StockMovement movement, Product product, Warehouse warehouse) => new(
        movement.Id, product.Id, product.Name, warehouse.Id, warehouse.Name,
        movement.MovementType.ToString(), movement.Quantity, movement.Reference, movement.MovementDate);
}