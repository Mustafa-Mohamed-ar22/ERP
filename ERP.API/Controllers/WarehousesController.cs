using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    public WarehousesController(IWarehouseService warehouseService) => _warehouseService = warehouseService;

    [HttpGet]
    [Authorize(Policy = "inventory.warehouses.view")]
    public async Task<ActionResult<List<WarehouseResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _warehouseService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "inventory.warehouses.view")]
    public async Task<ActionResult<WarehouseResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _warehouseService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "inventory.warehouses.manage")]
    public async Task<ActionResult<WarehouseResponse>> CreateAsync(
        [FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _warehouseService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "inventory.warehouses.manage")]
    public async Task<ActionResult<WarehouseResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _warehouseService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "inventory.warehouses.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _warehouseService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}
