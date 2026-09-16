public class Employee : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public string JobTitle { get; set; } = default!;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public decimal BaseSalary { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public Guid? UserId { get; set; }   // optional, unenforced link to ApplicationUser — see note above

    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}