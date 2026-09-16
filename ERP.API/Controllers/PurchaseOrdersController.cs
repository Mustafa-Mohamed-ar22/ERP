using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService) => _purchaseOrderService = purchaseOrderService;

    [HttpGet]
    [Authorize(Policy = "purchasing.orders.view")]
    public async Task<ActionResult<List<PurchaseOrderResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "purchasing.orders.view")]
    public async Task<ActionResult<PurchaseOrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "purchasing.orders.create")]
    public async Task<ActionResult<PurchaseOrderResponse>> CreateAsync(
        [FromBody] CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "purchasing.orders.create")]
    public async Task<ActionResult<PurchaseOrderResponse>> UpdateAsync(
    Guid id, [FromBody] CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "purchasing.orders.create")]
    public async Task<ActionResult<PurchaseOrderResponse>> SubmitAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.SubmitAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "purchasing.orders.approve")]
    public async Task<ActionResult<PurchaseOrderResponse>> ApproveAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.ApproveAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = "purchasing.orders.receive")]
    public async Task<ActionResult<PurchaseOrderResponse>> ReceiveGoodsAsync(
        Guid id, [FromBody] ReceiveGoodsRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.ReceiveGoodsAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "purchasing.orders.cancel")]
    public async Task<ActionResult<PurchaseOrderResponse>> CancelAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _purchaseOrderService.CancelAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}