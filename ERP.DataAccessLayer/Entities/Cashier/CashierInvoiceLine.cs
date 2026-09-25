public class CashierInvoiceLine : BaseEntity
{
    public Guid CashierInvoiceId { get; set; }
    public CashierInvoice CashierInvoice { get; set; } = default!;
    public Guid ProductId { get; set; }
    public string ProductNameSnapshot { get; set; } = default!;
    public string SkuSnapshot { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}