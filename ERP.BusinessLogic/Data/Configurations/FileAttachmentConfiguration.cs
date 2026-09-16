using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.ToTable("FileAttachments");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(255);
        builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(f => f.ContentType).IsRequired().HasMaxLength(100);
        builder.HasIndex(f => new { f.EntityName, f.EntityId });
    }
}
