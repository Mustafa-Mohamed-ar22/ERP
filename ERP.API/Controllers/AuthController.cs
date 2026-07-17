using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> LoginAsync(
        [FromBody] LoginRequest loginRequest, CancellationToken cancellationToken = default!)
    {
        var result = await _authService.LoginAsync(loginRequest, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> LogoutAsync(CancellationToken cancellationToken = default!)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _authService.RevokeTokenAsync(userId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}