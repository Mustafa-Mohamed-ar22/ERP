public class CashierPayment : BaseEntity
{
    public Guid CashierOrderId { get; set; }
    public CashierOrder CashierOrder { get; set; } = default!;
    public CashierPaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }       // card transaction reference, etc.
}
