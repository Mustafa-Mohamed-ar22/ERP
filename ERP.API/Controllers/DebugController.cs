using Microsoft.AspNetCore.Mvc;

public class DebugController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantProvider _tenantProvider;

    public DebugController(ICurrentUserService currentUser, ITenantProvider tenantProvider)
    {
        _currentUser = currentUser;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("debug/tenant")]
    public IActionResult Check() => Ok(new
    {
        CurrentUserCompanyId = _currentUser.CompanyId,
        TenantProviderCompanyId = _tenantProvider.CompanyId
    });
}