public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default); 
    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request,CancellationToken ct=default!);
    Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken ct = default!);
    Task<Result> SendResetPasswordCodeAsync(string email, CancellationToken ct = default!);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default!);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<Result> RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}