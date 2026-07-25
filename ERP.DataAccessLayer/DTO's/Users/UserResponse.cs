public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    Guid? BranchId,
    string? BranchName,
    Guid? DepartmentId,
    string? DepartmentName,
    bool IsActive,
    List<string> Roles);