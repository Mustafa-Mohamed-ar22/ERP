using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;
    public BranchesController(IBranchService branchService) => _branchService = branchService;

    [HttpGet]
    public async Task<ActionResult<List<BranchResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _branchService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchResponse>> GetByIdAsync([FromRoute]Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _branchService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "core.branches.manage")]
    public async Task<ActionResult<BranchResponse>> CreateAsync(
        [FromBody] CreateBranchRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _branchService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "core.branches.manage")]
    public async Task<ActionResult<BranchResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateBranchRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _branchService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "core.branches.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _branchService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}