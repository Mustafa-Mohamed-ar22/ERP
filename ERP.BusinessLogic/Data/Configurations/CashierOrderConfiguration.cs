using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashierOrderConfiguration : IEntityTypeConfiguration<CashierOrder>
{
    public void Configure(EntityTypeBuilder<CashierOrder> builder)
    {
        builder.ToTable("CashierOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.ChangeDue).HasColumnType("decimal(18,2)");
        builder.Property(o => o.VoidReason).HasMaxLength(300);
        builder.Property(x => x.WalkInCustomerPhone).HasMaxLength(11);

        builder.HasIndex(o => new { o.CompanyId, o.OrderNumber }).IsUnique().HasSoftDeleteFilter();

        builder.HasOne(o => o.CashierShift).WithMany(s => s.Orders).HasForeignKey(o => o.CashierShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Warehouse).WithMany().HasForeignKey(o => o.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.JournalEntry).WithMany().HasForeignKey(o => o.JournalEntryId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Lines).WithOne(l => l.CashierOrder).HasForeignKey(l => l.CashierOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.Payments).WithOne(p => p.CashierOrder).HasForeignKey(p => p.CashierOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(o => o.Invoice).WithOne(i => i.CashierOrder).HasForeignKey<CashierInvoice>(i => i.CashierOrderId);
    }
}