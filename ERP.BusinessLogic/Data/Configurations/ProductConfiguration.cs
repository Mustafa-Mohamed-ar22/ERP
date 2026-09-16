using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(20);
        builder.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
        builder.HasIndex(p => new { p.CompanyId, p.Sku }).IsUnique().HasSoftDeleteFilter(); ;
        builder.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique().HasSoftDeleteFilter(); ;
        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}