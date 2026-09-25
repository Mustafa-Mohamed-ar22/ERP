using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashierPaymentConfiguration : IEntityTypeConfiguration<CashierPayment>
{
    public void Configure(EntityTypeBuilder<CashierPayment> builder)
    {
        builder.ToTable("CashierPayments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ReferenceNumber).HasMaxLength(100);
    }
}