public record DepartmentResponse(
    Guid Id, string Name, Guid? BranchId, Guid? ParentDepartmentId, bool IsActive);
