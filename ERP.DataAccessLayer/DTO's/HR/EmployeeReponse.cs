public record EmployeeResponse(Guid Id, string EmployeeCode, string FullName, string? NationalId, DateTime? DateOfBirth, DateTime HireDate, string JobTitle, Guid? DepartmentId, Guid? BranchId, Guid? ManagerId, decimal BaseSalary, string? Email, string? Phone, string? Address, string Status, Guid? UserId);
public record CreateEmployeeRequest(string EmployeeCode, string FullName, string? NationalId, DateTime? DateOfBirth, DateTime HireDate, string JobTitle, Guid? DepartmentId, Guid? BranchId, Guid? ManagerId, decimal BaseSalary, string? Email, string? Phone, string? Address, Guid? UserId);
public record UpdateEmployeeRequest(string FullName, string? NationalId, DateTime? DateOfBirth, string JobTitle, Guid? DepartmentId, Guid? BranchId, Guid? ManagerId, decimal BaseSalary, string? Email, string? Phone, string? Address, string Status, Guid? UserId);

public record LeaveRequestResponse(Guid Id, Guid EmployeeId, string EmployeeName, string LeaveType, DateTime StartDate, DateTime EndDate, string? Reason, string Status, DateTime? ApprovedAt);
public record CreateLeaveRequestRequest(Guid EmployeeId, string LeaveType, DateTime StartDate, DateTime EndDate, string? Reason);

public record AttendanceRecordResponse(Guid Id, Guid EmployeeId, string EmployeeName, DateTime Date, DateTime? CheckInTime, DateTime? CheckOutTime, string Status, string? Notes);


public record GrantEmployeeAccessRequest(string Email, List<string>? RoleNames);
