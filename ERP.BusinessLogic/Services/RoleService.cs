using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _roleManager = roleManager;
        _context = context;
        _currentUser = currentUser;
    }

    // ApplicationRole isn't tenant-filtered at the DB level (same reasoning as ApplicationUser),
    // so every query here filters by CompanyId explicitly — and deliberately excludes the global
    // SuperAdmin role (CompanyId == null), which has no place in a company admin's role list.

    public async Task<Result<List<RoleResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var roles = await _roleManager.Roles.IgnoreQueryFilters()
            .Where(r => r.CompanyId == _currentUser.CompanyId)
            .ToListAsync(ct);

        var responses = new List<RoleResponse>();
        foreach (var role in roles)
        {
            var permissions = await GetPermissionCodesAsync(role.Id, ct);
            responses.Add(new RoleResponse(role.Id, role.Name!, role.Description, permissions));
        }
        return Result.Success(responses);
    }

    public async Task<Result<RoleResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _roleManager.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == _currentUser.CompanyId, ct);

        if (role is null)
            return Result.Failure<RoleResponse>(RoleErrors.NotFound);

        var permissions = await GetPermissionCodesAsync(role.Id, ct);
        return Result.Success(new RoleResponse(role.Id, role.Name!, role.Description, permissions));
    }

    public async Task<Result<List<PermissionResponse>>> GetPermissionsCatalogAsync(CancellationToken ct = default)
    {
        var permissions = await _context.Permissions.IgnoreQueryFilters()
            .OrderBy(p => p.Module).ThenBy(p => p.Code)
            .Select(p => new PermissionResponse(p.Code, p.Module, p.Description))
            .ToListAsync(ct);

        return Result.Success(permissions);
    }

    public async Task<Result<RoleResponse>> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var validPermissionCodes = await ValidatePermissionCodesAsync(request.PermissionCodes, ct);
        if (validPermissionCodes is null)
            return Result.Failure<RoleResponse>(RoleErrors.InvalidPermissions);

        var role = new ApplicationRole
        {
            Name = request.Name,
            CompanyId = _currentUser.CompanyId,
            Description = request.Description
        };

        var createResult = await _roleManager.CreateAsync(role); // TenantRoleValidator enforces per-company uniqueness
        if (!createResult.Succeeded)
        {
            var error = createResult.Errors.First();
            return Result.Failure<RoleResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        await AttachPermissionsAsync(role.Id, validPermissionCodes, ct);

        return Result.Success(new RoleResponse(role.Id, role.Name!, role.Description, validPermissionCodes));
    }

    public async Task<Result<RoleResponse>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var role = await _roleManager.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == _currentUser.CompanyId, ct);

        if (role is null)
            return Result.Failure<RoleResponse>(RoleErrors.NotFound);

        role.Name = request.Name;
        role.Description = request.Description;

        var updateResult = await _roleManager.UpdateAsync(role); // re-normalizes name, re-runs the validator
        if (!updateResult.Succeeded)
        {
            var error = updateResult.Errors.First();
            return Result.Failure<RoleResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        var permissions = await GetPermissionCodesAsync(role.Id, ct);
        return Result.Success(new RoleResponse(role.Id, role.Name!, role.Description, permissions));
    }

    public async Task<Result<RoleResponse>> UpdatePermissionsAsync(Guid id, AssignRolePermissionsRequest request, CancellationToken ct = default)
    {
        var role = await _roleManager.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == _currentUser.CompanyId, ct);

        if (role is null)
            return Result.Failure<RoleResponse>(RoleErrors.NotFound);

        var validPermissionCodes = await ValidatePermissionCodesAsync(request.PermissionCodes, ct);
        if (validPermissionCodes is null)
            return Result.Failure<RoleResponse>(RoleErrors.InvalidPermissions);

        // Full replace — remove everything currently attached, then attach exactly the requested set
        var existingLinks = await _context.RolePermissions.IgnoreQueryFilters()
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync(ct);
        _context.RolePermissions.RemoveRange(existingLinks);

        await AttachPermissionsAsync(role.Id, validPermissionCodes, ct);

        return Result.Success(new RoleResponse(role.Id, role.Name!, role.Description, validPermissionCodes));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _roleManager.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == _currentUser.CompanyId, ct);

        if (role is null)
            return Result.Failure(RoleErrors.NotFound);

        var hasUsers = await _context.UserRoles.IgnoreQueryFilters().AnyAsync(ur => ur.RoleId == id, ct);
        if (hasUsers)
            return Result.Failure(RoleErrors.CannotDeleteRoleWithUsers);

        await _roleManager.DeleteAsync(role);
        return Result.Success();
    }

    private async Task<List<string>?> ValidatePermissionCodesAsync(List<string> codes, CancellationToken ct)
    {
        var distinctCodes = codes.Distinct().ToList();
        var existingCodes = await _context.Permissions.IgnoreQueryFilters()
            .Where(p => distinctCodes.Contains(p.Code))
            .Select(p => p.Code)
            .ToListAsync(ct);

        return existingCodes.Count == distinctCodes.Count ? existingCodes : null;
    }

    private async Task AttachPermissionsAsync(Guid roleId, List<string> permissionCodes, CancellationToken ct)
    {
        var permissionIds = await _context.Permissions.IgnoreQueryFilters()
            .Where(p => permissionCodes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync(ct);

        _context.RolePermissions.AddRange(permissionIds.Select(pid => new RolePermission { RoleId = roleId, PermissionId = pid }));
        await _context.SaveChangesAsync(ct);
    }

    private async Task<List<string>> GetPermissionCodesAsync(Guid roleId, CancellationToken ct)
    {
        return await _context.RolePermissions.IgnoreQueryFilters()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Code)
            .ToListAsync(ct);
    }
}