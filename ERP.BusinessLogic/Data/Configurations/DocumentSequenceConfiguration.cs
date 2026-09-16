using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("DocumentSequences");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DocumentType).IsRequired().HasMaxLength(50);
        builder.HasIndex(d => new { d.CompanyId, d.DocumentType }).IsUnique();
    }
}