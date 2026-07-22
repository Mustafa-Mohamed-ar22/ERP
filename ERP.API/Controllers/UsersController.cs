using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    [Authorize(Policy = "core.users.manage")]
    public async Task<ActionResult<List<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _userService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "core.users.manage")]
    public async Task<ActionResult<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "core.users.manage")]
    public async Task<ActionResult<UserResponse>> CreateAsync(
        [FromBody] CreateUserRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _userService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "core.users.manage")]
    public async Task<ActionResult<UserResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _userService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = "core.roles.manage")]
    public async Task<ActionResult> AssignRolesAsync(Guid id, [FromBody] AssignUserRolesRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _userService.AssignRolesAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "core.users.manage")]
    public async Task<ActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _userService.DeactivateAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}