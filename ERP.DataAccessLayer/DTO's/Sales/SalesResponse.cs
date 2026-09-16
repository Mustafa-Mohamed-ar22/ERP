public record CustomerResponse(Guid Id, string Name, string? ContactName, string? Phone, string? Email, string? Address, string? TaxNumber, bool IsActive);
public record CreateCustomerRequest(string Name, string? ContactName, string? Phone, string? Email, string? Address, string? TaxNumber);
public record UpdateCustomerRequest(string Name, string? ContactName, string? Phone, string? Email, string? Address, string? TaxNumber, bool IsActive);

public record SalesOrderLineRequest(Guid ProductId, decimal Quantity, decimal UnitPrice);
public record SalesOrderLineResponse(Guid Id, Guid ProductId, string ProductSku, string ProductName, decimal Quantity, decimal UnitPrice, decimal ShippedQuantity);
public record CreateSalesOrderRequest(Guid CustomerId, Guid WarehouseId, DateTime OrderDate, string? Notes, List<SalesOrderLineRequest> Lines);
public record SalesOrderResponse(
    Guid Id, string OrderNumber, Guid CustomerId, string CustomerName, Guid WarehouseId, string WarehouseName,
    DateTime OrderDate, string Status, string? Notes, decimal TotalAmount,
    List<SalesOrderLineResponse> Lines, List<string> Warnings);
public record ShipLineRequest(Guid LineId, decimal QuantityShipped);
public record ShipGoodsRequest(List<ShipLineRequest> Lines);