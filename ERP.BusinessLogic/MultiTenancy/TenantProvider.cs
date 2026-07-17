using Microsoft.AspNetCore.Http;

public class TenantProvider : ITenantProvider
{
    public Guid CompanyId { get; }

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        var claim = httpContextAccessor.HttpContext?.User?.FindFirst("companyId")?.Value;
        CompanyId = claim != null ? Guid.Parse(claim) : Guid.Empty;
    }
}