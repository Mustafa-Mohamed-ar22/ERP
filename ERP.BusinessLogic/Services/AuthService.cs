using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly JwtProvider _jwtProvider;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        JwtProvider jwtProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _context = context;
        _jwtProvider = jwtProvider;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result.Failure<AuthResponse>(AuthErrors.UserInactive);

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _jwtProvider.GenerateAccessToken(user, user.CompanyId, roles);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
        });

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponse(
            accessToken, refreshToken, expiresAt, user.Id, user.FullName, user.CompanyId, roles.ToList()));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtProvider.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

        var storedToken = await _context.RefreshTokens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);
        if (!user.IsActive)
            return Result.Failure<AuthResponse>(AuthErrors.UserInactive);

        storedToken.IsRevoked = true;

        var roles = await _userManager.GetRolesAsync(user);
        var (newAccessToken, expiresAt) = _jwtProvider.GenerateAccessToken(user, user.CompanyId, roles);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponse(
            newAccessToken, newRefreshToken, expiresAt, user.Id, user.FullName, user.CompanyId, roles.ToList()));
    }

    public async Task<Result> RevokeTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens.IgnoreQueryFilters()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        tokens.ForEach(t => t.IsRevoked = true);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}