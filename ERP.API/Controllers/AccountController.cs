using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    public AccountController(IPermissionService permissionService)
        => _permissionService = permissionService;

    [HttpGet("me/permissions")]
    [Authorize] // any authenticated user, roled or not
    public async Task<ActionResult<List<string>>> GetMyPermissionsAsync(CancellationToken cancellationToken)
    {
        var roleNames = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var permissions = await _permissionService.GetPermissionsForRolesAsync(roleNames, cancellationToken);
        return Ok(permissions); // [] for a roleless employee
    }
}