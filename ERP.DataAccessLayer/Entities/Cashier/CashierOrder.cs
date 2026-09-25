public class CashierOrder : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid CashierShiftId { get; set; }
    public CashierShift CashierShift { get; set; } = default!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? WalkInCustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public CashierOrderStatus Status { get; set; } = CashierOrderStatus.Completed;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ChangeDue { get; set; }
    public Guid? JournalEntryId { get; set; }          // captured properly this time — Void can actually reverse it
    public JournalEntry? JournalEntry { get; set; }
    public DateTime? VoidedAt { get; set; }
    public Guid? VoidedByUserId { get; set; }
    public string? VoidReason { get; set; }

    public ICollection<CashierOrderLine> Lines { get; set; } = new List<CashierOrderLine>();
    public ICollection<CashierPayment> Payments { get; set; } = new List<CashierPayment>();
    public CashierInvoice? Invoice { get; set; }


    public string? WalkInCustomerPhone { get; set; }

}
