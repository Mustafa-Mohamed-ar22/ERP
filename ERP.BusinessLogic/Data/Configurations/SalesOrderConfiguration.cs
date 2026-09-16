using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.OrderNumber).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(s => new { s.CompanyId, s.OrderNumber }).IsUnique().HasSoftDeleteFilter();

        builder.HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Lines)
            .WithOne(l => l.SalesOrder)
            .HasForeignKey(l => l.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
