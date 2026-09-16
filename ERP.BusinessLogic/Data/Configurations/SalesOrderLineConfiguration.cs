using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.ShippedQuantity).HasColumnType("decimal(18,3)");

        builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);

        // Applying the lesson from PurchaseOrderLine's warning up front, instead of waiting for the same
        // EF model-validation warning to reappear on this table too — Product and SalesOrder are both
        // required + filtered, so mirror that filter here.
        builder.HasQueryFilter(l => !l.Product.IsDeleted && !l.SalesOrder.IsDeleted);
    }
}