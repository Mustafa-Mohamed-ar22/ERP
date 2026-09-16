using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    public SuppliersController(ISupplierService supplierService) => _supplierService = supplierService;

    [HttpGet]
    [Authorize(Policy = "purchasing.suppliers.view")]
    public async Task<ActionResult<List<SupplierResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _supplierService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "purchasing.suppliers.view")]
    public async Task<ActionResult<SupplierResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _supplierService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "purchasing.suppliers.manage")]
    public async Task<ActionResult<SupplierResponse>> CreateAsync(
        [FromBody] CreateSupplierRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _supplierService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "purchasing.suppliers.manage")]
    public async Task<ActionResult<SupplierResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _supplierService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "purchasing.suppliers.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _supplierService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}