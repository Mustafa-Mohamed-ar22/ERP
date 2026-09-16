using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    public CustomersController(ICustomerService customerService) => _customerService = customerService;

    [HttpGet]
    [Authorize(Policy = "sales.customers.view")]
    public async Task<ActionResult<List<CustomerResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _customerService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "sales.customers.view")]
    public async Task<ActionResult<CustomerResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "sales.customers.manage")]
    public async Task<ActionResult<CustomerResponse>> CreateAsync(
        [FromBody] CreateCustomerRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _customerService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "sales.customers.manage")]
    public async Task<ActionResult<CustomerResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _customerService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "sales.customers.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _customerService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}
