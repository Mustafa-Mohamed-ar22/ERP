public record SupplierResponse(Guid Id, string Name, string? ContactName, string? Phone, string? Email, string? Address, string? TaxNumber, bool IsActive);
public record CreateSupplierRequest(string Name, string? ContactName, string? Phone, string? Email, string? Address, string? TaxNumber);
public record UpdateSupplierRequest(string Name, string? ContactName, string? Phone, string? Email, string? Address, string? TaxNumber, bool IsActive);

public record PurchaseOrderLineRequest(Guid ProductId, decimal Quantity, decimal UnitPrice);
public record PurchaseOrderLineResponse(Guid Id, Guid ProductId, string ProductSku, string ProductName, decimal Quantity, decimal UnitPrice, decimal ReceivedQuantity);
public record CreatePurchaseOrderRequest(Guid SupplierId, Guid WarehouseId, DateTime OrderDate, string? Notes, List<PurchaseOrderLineRequest> Lines);
public record PurchaseOrderResponse(
    Guid Id, string OrderNumber, Guid SupplierId, string SupplierName, Guid WarehouseId, string WarehouseName,
    DateTime OrderDate, string Status, string? Notes, decimal TotalAmount,
    List<PurchaseOrderLineResponse> Lines, List<string> Warnings);
public record ReceiveLineRequest(Guid LineId, decimal QuantityReceived);
public record ReceiveGoodsRequest(List<ReceiveLineRequest> Lines);