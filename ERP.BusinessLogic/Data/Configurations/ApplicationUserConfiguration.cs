using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.HasIndex(u => new { u.CompanyId, u.Email });



        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("UserRefreshTokens");
            rt.WithOwner().HasForeignKey("UserId");
            rt.Property<int>("Id");
            rt.HasKey("Id");
            rt.Property(r => r.Token).IsRequired().HasMaxLength(500);
            rt.HasIndex(r => r.Token);
        });

        builder.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}