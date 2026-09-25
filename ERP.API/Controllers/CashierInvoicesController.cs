using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/cashier/invoices")]
[Authorize]
public class CashierInvoicesController : ControllerBase
{
    private readonly ICashierOrderService _service;
    public CashierInvoicesController(ICashierOrderService service) => _service = service;

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "cashier.invoices.view")]
    public async Task<ActionResult<CashierInvoiceResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetInvoiceByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpGet("my-invoices")]
    public async Task<ActionResult<List<CashierInvoiceResponse>>> GetMyInvoicesAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetMyInvoicesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("by-user/{userId:guid}")]
    [Authorize(Policy = "cashier.invoices.viewall")]
    public async Task<ActionResult<List<CashierInvoiceResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetInvoicesByUserIdAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}