using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
        => _permissionService = permissionService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var roleNames = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (roleNames.Count == 0) return; // roleless employee — no permission-based policy passes, which is correct

        var permissions = await _permissionService.GetPermissionsForRolesAsync(roleNames);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}