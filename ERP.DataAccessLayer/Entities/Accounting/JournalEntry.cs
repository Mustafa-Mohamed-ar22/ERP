public class JournalEntry : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string EntryNumber { get; set; } = default!;
    public DateTime EntryDate { get; set; }
    public string? Description { get; set; }
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }

    // Set only on a reversal entry, pointing back at the entry it reverses.
    // The original entry's Status never changes to anything but Posted — its GL effect is
    // canceled out by the reversal's opposite-signed lines, which is the standard accounting practice.
    public Guid? ReversalOfEntryId { get; set; }
    public JournalEntry? ReversalOfEntry { get; set; }

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
