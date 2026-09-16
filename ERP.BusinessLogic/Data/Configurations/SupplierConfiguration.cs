using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ContactName).HasMaxLength(150);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.Address).HasMaxLength(300);
        builder.Property(s => s.TaxNumber).HasMaxLength(50);
        builder.HasIndex(x => new { x.CompanyId, x.Phone }).IsUnique().HasSoftDeleteFilter(); ;
        builder.HasIndex(x => new { x.CompanyId, x.Email}).IsUnique() .HasSoftDeleteFilter();;
    }
}
