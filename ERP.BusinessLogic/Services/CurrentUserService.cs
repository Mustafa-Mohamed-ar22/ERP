using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    public Guid UserId { get; }
    public Guid CompanyId { get; }
    public string? Email { get; }
    public IEnumerable<string> Roles { get; }
    public bool IsAuthenticated { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;

        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        UserId = Guid.TryParse(userIdClaim, out var uid) ? uid : Guid.Empty;

        var companyIdClaim = user?.FindFirst("companyId")?.Value;
        CompanyId = Guid.TryParse(companyIdClaim, out var cid) ? cid : Guid.Empty;

        Email = user?.FindFirst(ClaimTypes.Email)?.Value;
        Roles = user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
    }
}