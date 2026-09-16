using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.OrderNumber).IsRequired().HasMaxLength(30);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(p => new { p.CompanyId, p.OrderNumber }).IsUnique().HasSoftDeleteFilter();

        builder.HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Warehouse).WithMany().HasForeignKey(p => p.WarehouseId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Lines)
            .WithOne(l => l.PurchaseOrder)
            .HasForeignKey(l => l.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
