using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class JwtProvider
{
    private readonly JwtOptions _options;
    public JwtProvider(IOptions<JwtOptions> options) => _options = options.Value;
    public string GenerateAccessTaoken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName),
            new("companyId", user.CompanyId.ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static RefreshToken GenerateRefreshToken() => new()
    {
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        CreatedOn = DateTime.UtcNow,
        ExpiresOn = DateTime.UtcNow.AddDays(14)
    };
}