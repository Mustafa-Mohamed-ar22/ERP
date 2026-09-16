using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Address).HasMaxLength(300);
        builder.Property(b => b.Phone).HasMaxLength(30);
        builder.HasIndex(b => new { b.CompanyId, b.Code }).IsUnique().HasSoftDeleteFilter(); ;
        builder.HasIndex(x => new {x.CompanyId ,x.Name }).IsUnique().HasSoftDeleteFilter(); ;
        builder.HasIndex(x => new {x.CompanyId ,x.Phone }).IsUnique().HasSoftDeleteFilter(); ;
        
        builder.HasMany(b => b.Departments)
            .WithOne(d => d.Branch)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
