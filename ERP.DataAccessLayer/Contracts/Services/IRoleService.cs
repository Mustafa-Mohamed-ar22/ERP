public interface IRoleService
{
    Task<Result<List<RoleResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<RoleResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<PermissionResponse>>> GetPermissionsCatalogAsync(CancellationToken ct = default);
    Task<Result<RoleResponse>> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task<Result<RoleResponse>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default);
    Task<Result<RoleResponse>> UpdatePermissionsAsync(Guid id, AssignRolePermissionsRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}