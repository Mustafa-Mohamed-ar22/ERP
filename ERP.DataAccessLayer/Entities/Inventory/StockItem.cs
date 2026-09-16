public class StockItem : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public decimal QuantityOnHand { get; set; }

    // Optimistic concurrency token 
    public byte[] RowVersion { get; set; } = default!;
}
