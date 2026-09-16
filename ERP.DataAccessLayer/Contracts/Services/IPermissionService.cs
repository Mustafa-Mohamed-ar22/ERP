public interface IPermissionService
{
    Task<List<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames, CancellationToken ct = default);
}