public class LeaveRequest : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}