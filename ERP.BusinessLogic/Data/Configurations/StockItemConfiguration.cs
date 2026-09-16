using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.QuantityOnHand).HasColumnType("decimal(18,3)");
        builder.HasIndex(s => new { s.CompanyId, s.ProductId, s.WarehouseId }).IsUnique().HasSoftDeleteFilter(); ;
        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}