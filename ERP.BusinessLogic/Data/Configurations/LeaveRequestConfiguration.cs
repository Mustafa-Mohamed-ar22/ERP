using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LeaveType).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Reason).HasMaxLength(500);

        builder.HasOne(l => l.Employee).WithMany(e => e.LeaveRequests).HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(l => !l.Employee.IsDeleted);
    }
}
