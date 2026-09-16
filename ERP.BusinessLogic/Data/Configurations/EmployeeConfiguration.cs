using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(30);
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(150);
        builder.Property(e => e.NationalId).HasMaxLength(50);
        builder.Property(e => e.JobTitle).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Address).HasMaxLength(300);
        builder.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => new { e.CompanyId, e.EmployeeCode }).IsUnique().HasSoftDeleteFilter();
        builder.HasIndex(e => e.UserId); // no FK constraint 
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => new { e.CompanyId, e.Email }).IsUnique().HasSoftDeleteFilter();
        builder.HasIndex(e => new { e.CompanyId, e.Phone }).IsUnique().HasSoftDeleteFilter();
        builder.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict); // self-referencing FK must be Restrict
    }
}
