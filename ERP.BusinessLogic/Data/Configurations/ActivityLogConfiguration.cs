// Data/Configurations/ActivityLogConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// Data/Configurations/ActivityLogConfiguration.cs
public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(500);
        builder.Property(a => a.Module).IsRequired().HasMaxLength(50);
    }
}
