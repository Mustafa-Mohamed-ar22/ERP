using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> LoginAsync(
        [FromBody] LoginRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponse>> RegisterAsync(
        [FromBody] RegisterRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        var result = await _authService.ConfirmEmailAsync(request);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("resend-confirmation-email")]
    [AllowAnonymous]
    public async Task<ActionResult> ResendConfirmationEmailAsync([FromBody] ResendConfirmationEmailRequest request)
    {
        var result = await _authService.ResendConfirmationEmailAsync(request);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.SendResetPasswordCodeAsync(request.Email);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshTokenAsync(
        [FromBody] string refreshToken, CancellationToken cancellationToken = default!)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<ActionResult> RevokeTokenAsync(
        [FromBody] string refreshToken, CancellationToken cancellationToken = default!)
    {
        var result = await _authService.RevokeTokenAsync(refreshToken, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}