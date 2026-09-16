using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(s => s.MovementType).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.Reference).HasMaxLength(200);

        builder.HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.RelatedWarehouse).WithMany().HasForeignKey(s => s.RelatedWarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}

