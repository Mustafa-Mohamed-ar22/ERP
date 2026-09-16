using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Notes).HasMaxLength(300);
        builder.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique(); // one record per employee per day

        builder.HasOne(a => a.Employee).WithMany(e => e.AttendanceRecords).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(a => !a.Employee.IsDeleted);
    }
}