using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationDbContext _context;

    public PermissionAuthorizationHandler(ApplicationDbContext context) => _context = context;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return;

        var roleNames = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (roleNames.Count == 0) return;

        var hasPermission = await _context.RolePermissions
            .IgnoreQueryFilters()
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .AnyAsync(rp => roleNames.Contains(rp.Role.Name!) && rp.Permission.Code == requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);
    }
}