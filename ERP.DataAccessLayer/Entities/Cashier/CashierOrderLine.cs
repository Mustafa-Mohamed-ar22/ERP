public class CashierOrderLine : BaseEntity
{
    public Guid CashierOrderId { get; set; }
    public CashierOrder CashierOrder { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }              // snapshotted Product.CostPrice at sale time — needed for COGS posting
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}
