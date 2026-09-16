using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountingSettingsController : ControllerBase
{
    private readonly IAccountingSettingsService _service;
    public AccountingSettingsController(IAccountingSettingsService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = "accounting.settings.manage")]
    public async Task<ActionResult<AccountingSettingsResponse>> GetAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut]
    [Authorize(Policy = "accounting.settings.manage")]
    public async Task<ActionResult<AccountingSettingsResponse>> UpdateAsync(
        [FromBody] UpdateAccountingSettingsRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _service.UpdateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}