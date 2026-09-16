using Microsoft.EntityFrameworkCore;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    public PermissionService(ApplicationDbContext context) => _context = context;

    public async Task<List<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames, CancellationToken ct = default)
    {
        var roles = roleNames.ToList();
        if (roles.Count == 0) return new List<string>();

        return await _context.RolePermissions
            .IgnoreQueryFilters()
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .Where(rp => roles.Contains(rp.Role.Name!))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(ct);
    }
}