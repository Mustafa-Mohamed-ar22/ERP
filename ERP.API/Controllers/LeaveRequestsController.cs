using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    public LeaveRequestsController(ILeaveRequestService leaveRequestService) => _leaveRequestService = leaveRequestService;

    [HttpGet]
    [Authorize(Policy = "hr.leaves.view")]
    public async Task<ActionResult<List<LeaveRequestResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _leaveRequestService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "hr.leaves.view")]
    public async Task<ActionResult<LeaveRequestResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _leaveRequestService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "hr.leaves.request")]
    public async Task<ActionResult<LeaveRequestResponse>> CreateAsync(
        [FromBody] CreateLeaveRequestRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _leaveRequestService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "hr.leaves.approve")]
    public async Task<ActionResult<LeaveRequestResponse>> ApproveAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _leaveRequestService.ApproveAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "hr.leaves.approve")]
    public async Task<ActionResult<LeaveRequestResponse>> RejectAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _leaveRequestService.RejectAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "hr.leaves.request")]
    public async Task<ActionResult<LeaveRequestResponse>> CancelAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _leaveRequestService.CancelAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpGet("my-requests")]
    public async Task<ActionResult<List<LeaveRequestResponse>>> GetMyRequestsAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _leaveRequestService.GetMyRequestsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}