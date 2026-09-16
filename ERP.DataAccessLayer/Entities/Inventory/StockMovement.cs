public class StockMovement : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public Guid? RelatedWarehouseId { get; set; }   // the other side of a transfer
    public Warehouse? RelatedWarehouse { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }           // always positive; direction comes from MovementType
    public string? Reference { get; set; }
    public DateTime MovementDate { get; set; }
}