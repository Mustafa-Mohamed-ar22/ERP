using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/cashier/orders")]
[Authorize]
public class CashierOrdersController : ControllerBase
{
    private readonly ICashierOrderService _service;
    public CashierOrdersController(ICashierOrderService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = "cashier.orders.view")]
    public async Task<ActionResult<List<CashierOrderResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "cashier.orders.view")]
    public async Task<ActionResult<CashierOrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "cashier.orders.create")]
    public async Task<ActionResult<CashierOrderResponse>> CreateAsync([FromBody] CreateCashierOrderRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _service.CreateOrderAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/void")]
    [Authorize(Policy = "cashier.orders.void")]
    public async Task<ActionResult<CashierOrderResponse>> VoidAsync(Guid id, [FromBody] VoidCashierOrderRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _service.VoidOrderAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}/invoice")]
    [Authorize(Policy = "cashier.invoices.view")]
    public async Task<ActionResult<CashierInvoiceResponse>> GetInvoiceAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetInvoiceByOrderIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}
