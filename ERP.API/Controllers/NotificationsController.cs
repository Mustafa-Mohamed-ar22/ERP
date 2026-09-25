using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    public NotificationsController(INotificationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> GetMyNotificationsAsync(
        [FromQuery] bool? unreadOnly, CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetMyNotificationsAsync(unreadOnly, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetMyUnreadCountAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _service.GetMyUnreadCountAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/mark-read")]
    public async Task<ActionResult> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _service.MarkAsReadAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult> MarkAllAsReadAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _service.MarkAllAsReadAsync(cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}