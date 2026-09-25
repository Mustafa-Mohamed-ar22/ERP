using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashierInvoiceLineConfiguration : IEntityTypeConfiguration<CashierInvoiceLine>
{
    public void Configure(EntityTypeBuilder<CashierInvoiceLine> builder)
    {
        builder.ToTable("CashierInvoiceLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ProductNameSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(l => l.SkuSnapshot).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.LineTotal).HasColumnType("decimal(18,2)");
    }
}