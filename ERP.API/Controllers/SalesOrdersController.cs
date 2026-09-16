using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _salesOrderService;
    public SalesOrdersController(ISalesOrderService salesOrderService) => _salesOrderService = salesOrderService;

    [HttpGet]
    [Authorize(Policy = "sales.orders.view")]
    public async Task<ActionResult<List<SalesOrderResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "sales.orders.view")]
    public async Task<ActionResult<SalesOrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "sales.orders.create")]
    public async Task<ActionResult<SalesOrderResponse>> CreateAsync(
        [FromBody] CreateSalesOrderRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "sales.orders.create")]
    public async Task<ActionResult<SalesOrderResponse>> UpdateAsync(
    Guid id, [FromBody] CreateSalesOrderRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "sales.orders.create")]
    public async Task<ActionResult<SalesOrderResponse>> SubmitAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.SubmitAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "sales.orders.approve")]
    public async Task<ActionResult<SalesOrderResponse>> ApproveAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.ApproveAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPost("{id:guid}/ship")]
    [Authorize(Policy = "sales.orders.ship")]
    public async Task<ActionResult<SalesOrderResponse>> ShipGoodsAsync(
        Guid id, [FromBody] ShipGoodsRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.ShipGoodsAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "sales.orders.cancel")]
    public async Task<ActionResult<SalesOrderResponse>> CancelAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _salesOrderService.CancelAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}