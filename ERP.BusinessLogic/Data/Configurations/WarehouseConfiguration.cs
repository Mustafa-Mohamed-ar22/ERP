using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(150);
        builder.Property(w => w.Code).IsRequired().HasMaxLength(30);

        builder.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique().HasSoftDeleteFilter(); ;

        builder.HasIndex(w => new { w.CompanyId, w.Code })
               .IsUnique()
               .HasFilter("[IsDeleted] = 0");


        builder.HasOne(w => w.Branch)
            .WithMany()
            .HasForeignKey(w => w.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
