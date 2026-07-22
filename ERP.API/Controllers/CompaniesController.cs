using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    public CompaniesController(ICompanyService companyService) => _companyService = companyService;

    [HttpGet("me")]
    public async Task<ActionResult<CompanyResponse>> GetCurrentAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _companyService.GetCurrentAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("me")]
    [Authorize(Policy = "core.companies.manage")]
    public async Task<ActionResult<CompanyResponse>> UpdateCurrentAsync(
        [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _companyService.UpdateCurrentAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}