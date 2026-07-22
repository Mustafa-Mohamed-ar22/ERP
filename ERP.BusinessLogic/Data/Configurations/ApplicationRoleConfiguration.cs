using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Description).HasMaxLength(300);

        // Identity's base config puts a GLOBAL unique index on NormalizedName.
        // Override it here: every company needs its own "Admin"/"Manager"/etc.
        // System roles (CompanyId == null, e.g. SuperAdmin) stay globally unique via the composite index too,
        // since NULL is treated as distinct per row in SQL Server's unique index semantics.
        builder.HasIndex(r => r.NormalizedName).IsUnique(false);
        builder.HasIndex(r => new { r.CompanyId, r.NormalizedName }).IsUnique();
    }
}