using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Category).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => new { s.CompanyId, s.Key }).IsUnique().HasSoftDeleteFilter(); ;
    }
}
