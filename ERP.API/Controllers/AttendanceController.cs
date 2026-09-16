using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    public AttendanceController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

    [HttpGet]
    [Authorize(Policy = "hr.attendance.view")]
    public async Task<ActionResult<List<AttendanceRecordResponse>>> GetAllAsync(
        [FromQuery] Guid? employeeId, CancellationToken cancellationToken = default!)
    {
        var result = await _attendanceService.GetAllAsync(employeeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<AttendanceRecordResponse>> CheckInAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _attendanceService.CheckInAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("check-out")]
    public async Task<ActionResult<AttendanceRecordResponse>> CheckOutAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _attendanceService.CheckOutAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("my-history")]
    public async Task<ActionResult<List<AttendanceRecordResponse>>> GetMyHistoryAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _attendanceService.GetMyHistoryAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}