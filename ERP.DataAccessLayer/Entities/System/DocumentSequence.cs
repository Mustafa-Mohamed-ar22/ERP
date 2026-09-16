public class DocumentSequence : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string DocumentType { get; set; } = default!;  // "JournalEntry", "PurchaseOrder", "SalesOrder"
    public int LastNumber { get; set; } = 0;
}