using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    public RolesController(IRoleService roleService) => _roleService = roleService;

    [HttpGet]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult<List<RoleResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _roleService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("permissions-catalog")]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult<List<PermissionResponse>>> GetPermissionsCatalogAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _roleService.GetPermissionsCatalogAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult<RoleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _roleService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult<RoleResponse>> CreateAsync(
        [FromBody] CreateRoleRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _roleService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult<RoleResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult<RoleResponse>> UpdatePermissionsAsync(
        Guid id, [FromBody] AssignRolePermissionsRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _roleService.UpdatePermissionsAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _roleService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}