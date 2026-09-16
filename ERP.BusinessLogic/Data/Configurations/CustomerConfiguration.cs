using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;  
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ContactName).HasMaxLength(150);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.TaxNumber).HasMaxLength(50);
        builder.HasIndex(x => new { x.CompanyId, x.Phone }).IsUnique().HasSoftDeleteFilter(); 
        builder.HasIndex(x => new { x.CompanyId, x.Email }).IsUnique().HasSoftDeleteFilter(); 
    }
}