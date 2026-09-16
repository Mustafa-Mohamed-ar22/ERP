using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.EntryNumber).IsRequired().HasMaxLength(30);
        builder.Property(j => j.Description).HasMaxLength(500);
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(j => new { j.CompanyId, j.EntryNumber }).IsUnique().HasSoftDeleteFilter(); ;

        builder.HasOne(j => j.ReversalOfEntry)
            .WithMany()
            .HasForeignKey(j => j.ReversalOfEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.Lines)
            .WithOne(l => l.JournalEntry)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade); // safe: entries with lines are only ever deleted while still Draft
    }
}
