using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/cashier/shifts")]
[Authorize]
public class CashierShiftsController : ControllerBase
{
    private readonly ICashierShiftService _service;
    public CashierShiftsController(ICashierShiftService service) => _service = service;

    [HttpPost("open")]
    [Authorize(Policy = "cashier.shifts.open")]
    public async Task<ActionResult<CashierShiftResponse>> OpenAsync([FromBody] OpenShiftRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _service.OpenShiftAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/cash-movements")]
    [Authorize(Policy = "cashier.shifts.cashmovement")]
    public async Task<ActionResult> AddCashMovementAsync(Guid id, [FromBody] CashMovementRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _service.AddCashMovementAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "cashier.shifts.close")]
    public async Task<ActionResult<ShiftClosingReportResponse>> CloseAsync(Guid id, [FromBody] CloseShiftRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _service.CloseShiftAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}/closing-report")]
    [Authorize(Policy = "cashier.shifts.view")]
    public async Task<ActionResult<ShiftClosingReportResponse>> GetClosingReportAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetClosingReportAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "cashier.shifts.view")]
    public async Task<ActionResult<CashierShiftResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    // Self-service — no permission policy, same reasoning as attendance/leave "my-*" endpoints
    [HttpGet("my-current")]
    public async Task<ActionResult<CashierShiftResponse>> GetMyCurrentAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetMyCurrentShiftAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("my-history")]
    public async Task<ActionResult<List<CashierShiftResponse>>> GetMyHistoryAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetMyHistoryAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}
