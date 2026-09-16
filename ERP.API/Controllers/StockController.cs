using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;
    public StockController(IStockService stockService) => _stockService = stockService;

    [HttpGet("products/{productId:guid}")]
    [Authorize(Policy = "inventory.stock.view")]
    public async Task<ActionResult<List<StockLevelResponse>>> GetProductStockAsync(Guid productId, CancellationToken cancellationToken = default!)
    {
        var result = await _stockService.GetProductStockAsync(productId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpGet("warehouses/{warehouseId:guid}")]
    [Authorize(Policy = "inventory.stock.view")]
    public async Task<ActionResult<List<StockLevelResponse>>> GetWarehouseStockAsync(Guid warehouseId, CancellationToken cancellationToken = default!)
    {
        var result = await _stockService.GetWarehouseStockAsync(warehouseId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("movements")]
    [Authorize(Policy = "inventory.stock.adjust")]
    public async Task<ActionResult<StockMovementResponse>> RecordMovementAsync(
        [FromBody] RecordStockMovementRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _stockService.RecordMovementAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("transfer")]
    [Authorize(Policy = "inventory.stock.transfer")]
    public async Task<ActionResult<List<StockMovementResponse>>> TransferAsync(
        [FromBody] TransferStockRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _stockService.TransferAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}