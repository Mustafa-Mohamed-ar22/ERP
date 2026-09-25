using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashierInvoiceConfiguration : IEntityTypeConfiguration<CashierInvoice>
{
    public void Configure(EntityTypeBuilder<CashierInvoice> builder)
    {
        builder.ToTable("CashierInvoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(i => i.ChangeDue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.WalkInCustomerPhone).HasMaxLength(30);
        builder.HasIndex(i => new { i.CompanyId, i.InvoiceNumber }).IsUnique().HasSoftDeleteFilter();

        builder.HasOne(i => i.Customer).WithMany().HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(i => i.Lines).WithOne(l => l.CashierInvoice).HasForeignKey(l => l.CashierInvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}