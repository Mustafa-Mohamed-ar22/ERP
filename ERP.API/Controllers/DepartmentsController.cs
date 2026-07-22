using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    public DepartmentsController(IDepartmentService departmentService) => _departmentService = departmentService;

    [HttpGet]
    public async Task<ActionResult<List<DepartmentResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _departmentService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _departmentService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "core.departments.manage")]
    public async Task<ActionResult<DepartmentResponse>> CreateAsync(
        [FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _departmentService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "core.departments.manage")]
    public async Task<ActionResult<DepartmentResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _departmentService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "core.departments.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _departmentService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}