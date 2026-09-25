using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashierOrderLineConfiguration : IEntityTypeConfiguration<CashierOrderLine>
{
    public void Configure(EntityTypeBuilder<CashierOrderLine> builder)
    {
        builder.ToTable("CashierOrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(l => l.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.LineTotal).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        //builder.HasQueryFilter(l => !l.Product.IsDeleted && !l.CashierOrder.IsDeleted); // the now-familiar lesson, applied proactively
        

    }
}