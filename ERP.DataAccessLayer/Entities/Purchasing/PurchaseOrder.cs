public class PurchaseOrder : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;
    public Guid WarehouseId { get; set; }         // single destination warehouse per PO — simplification for v1
    public Warehouse Warehouse { get; set; } = default!;
    public DateTime OrderDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public string? Notes { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
