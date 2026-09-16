using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    public EmployeesController(IEmployeeService employeeService) => _employeeService = employeeService;

    [HttpGet]
    [Authorize(Policy = "hr.employees.view")]
    public async Task<ActionResult<List<EmployeeResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _employeeService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "hr.employees.view")]
    public async Task<ActionResult<EmployeeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _employeeService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "hr.employees.manage")]
    public async Task<ActionResult<EmployeeResponse>> CreateAsync(
        [FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _employeeService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "hr.employees.manage")]
    public async Task<ActionResult<EmployeeResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _employeeService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "hr.employees.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _employeeService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
    [HttpPost("{id:guid}/grant-access")]
    [Authorize(Policy = "hr.employees.manage")]
    public async Task<ActionResult<UserResponse>> GrantAccessAsync(
        Guid id, [FromBody] GrantEmployeeAccessRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _employeeService.GrantAccessAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}