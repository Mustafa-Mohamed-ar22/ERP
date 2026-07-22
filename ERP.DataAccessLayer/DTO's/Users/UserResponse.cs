public record UserResponse(
    Guid Id, string Email, string FullName, Guid? BranchId, Guid? DepartmentId, bool IsActive, List<string> Roles);
