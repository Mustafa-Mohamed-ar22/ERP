public class AttendanceRecord : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public DateTime Date { get; set; }             
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
}