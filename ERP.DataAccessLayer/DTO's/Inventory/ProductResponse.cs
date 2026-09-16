public record ProductResponse(
    Guid Id, string Sku, string Name, string? Description, string UnitOfMeasure,
    Guid? CategoryId, string? CategoryName, decimal CostPrice, decimal SalePrice, bool IsActive);

public record CreateProductRequest(string Sku, string Name, string? Description, string UnitOfMeasure, Guid? CategoryId, decimal CostPrice, decimal SalePrice);

public record UpdateProductRequest(string Sku, string Name, string? Description, string UnitOfMeasure, Guid? CategoryId, decimal CostPrice, decimal SalePrice, bool IsActive);
public record WarehouseResponse(Guid Id, string Name, string Code, Guid? BranchId, bool IsActive);
public record CreateWarehouseRequest(string Name, string Code, Guid? BranchId);
public record UpdateWarehouseRequest(string Name, string Code, Guid? BranchId, bool IsActive);

public record StockLevelResponse(Guid ProductId, string ProductSku, string ProductName, Guid WarehouseId, string WarehouseName, decimal QuantityOnHand);

public record RecordStockMovementRequest(Guid ProductId, Guid WarehouseId, string MovementType, decimal Quantity, string? Reference);

public record TransferStockRequest(Guid ProductId, Guid FromWarehouseId, Guid ToWarehouseId, decimal Quantity, string? Reference);

public record StockMovementResponse(Guid Id, Guid ProductId, string ProductName, Guid WarehouseId, string WarehouseName, string MovementType, decimal Quantity, string? Reference, DateTime MovementDate);



public record CategoryResponse(Guid Id, string Name, Guid? ParentCategoryId, bool IsActive);
public record CreateCategoryRequest(string Name, Guid? ParentCategoryId);
public record UpdateCategoryRequest(string Name, Guid? ParentCategoryId, bool IsActive);