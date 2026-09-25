public class CashierInvoice : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public Guid CashierOrderId { get; set; }
    public CashierOrder CashierOrder { get; set; } = default!;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? WalkInCustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public CashierInvoiceStatus Status { get; set; } = CashierInvoiceStatus.Issued;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeDue { get; set; }

    public ICollection<CashierInvoiceLine> Lines { get; set; } = new List<CashierInvoiceLine>();


    public string? WalkInCustomerPhone { get; set; }
}
