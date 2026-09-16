public record CreateUserRequest(string FullName, string Email, Guid? BranchId, Guid? DepartmentId, List<string>? RoleNames);
