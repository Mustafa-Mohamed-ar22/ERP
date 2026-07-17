public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request,CancellationToken cancellationToken=default!);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default!);
    Task<Result> RevokeTokenAsync(Guid userId, CancellationToken cancellationToken = default!);
}