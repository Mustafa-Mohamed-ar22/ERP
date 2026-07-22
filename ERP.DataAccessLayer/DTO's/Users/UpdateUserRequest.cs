public record UpdateUserRequest(string FullName, Guid? BranchId, Guid? DepartmentId, bool IsActive);
