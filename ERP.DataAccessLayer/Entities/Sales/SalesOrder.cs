public class SalesOrder : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public Guid WarehouseId { get; set; }          // source warehouse to ship from
    public Warehouse Warehouse { get; set; } = default!;
    public DateTime OrderDate { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public string? Notes { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
