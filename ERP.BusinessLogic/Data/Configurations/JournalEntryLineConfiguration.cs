using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Debit).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Credit).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Description).HasMaxLength(300);

        builder.HasQueryFilter(l => !l.Account.IsDeleted);

        builder.HasOne(l => l.Account)
            .WithMany(a => a.JournalEntryLines)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict); // never cascade-delete an account's transaction history
    }
}