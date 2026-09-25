using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    public LeaveRequestService
        (IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<Result<List<LeaveRequestResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var requests = await _unitOfWork.LeaveRequests.Query()
            .Include(l => l.Employee)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(ct);

        return Result.Success(requests.Select(ToResponse).ToList());
    }

    public async Task<Result<LeaveRequestResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _unitOfWork.LeaveRequests.Query()
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (request is null)
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NotFound);

        return Result.Success(ToResponse(request));
    }

    public async Task<Result<LeaveRequestResponse>> CreateAsync(CreateLeaveRequestRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<LeaveType>(request.LeaveType, true, out var leaveType))
            return Result.Failure<LeaveRequestResponse>(new Error("LeaveRequest.InvalidType", "Invalid leave type", "نوع الإجازة غير صالح", StatusCodes.Status400BadRequest));

        if (request.EndDate < request.StartDate)
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.InvalidDateRange);

        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, ct);
        if (employee is null)
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.EmployeeNotFound);

        var overlapping = await _unitOfWork.LeaveRequests.Query()
            .AnyAsync(l => l.EmployeeId == request.EmployeeId
                        && l.Status != LeaveRequestStatus.Rejected && l.Status != LeaveRequestStatus.Cancelled
                        && l.StartDate <= request.EndDate && l.EndDate >= request.StartDate, ct);

        if (overlapping)
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.OverlappingLeave);

        var leaveRequest = new LeaveRequest
        {
            CompanyId = _currentUser.CompanyId,
            EmployeeId = request.EmployeeId,
            LeaveType = leaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending
        };

        await _unitOfWork.LeaveRequests.AddAsync(leaveRequest, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(leaveRequest.Id, ct);
    }

    public async Task<Result<LeaveRequestResponse>> ApproveAsync(Guid id, CancellationToken ct = default)
        => await ResolveAsync(id, LeaveRequestStatus.Approved, ct);

    public async Task<Result<LeaveRequestResponse>> RejectAsync(Guid id, CancellationToken ct = default)
        => await ResolveAsync(id, LeaveRequestStatus.Rejected, ct);

    // LeaveRequestService.cs — inject INotificationService, update ResolveAsync (called by both Approve/Reject)
    private async Task<Result<LeaveRequestResponse>> ResolveAsync(Guid id, LeaveRequestStatus newStatus, CancellationToken ct)
    {
        var request = await _unitOfWork.LeaveRequests.Query().Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (request is null) return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NotFound);
        if (request.Status != LeaveRequestStatus.Pending) return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NotPending);

        request.Status = newStatus;
        request.ApprovedBy = _currentUser.UserId;
        request.ApprovedAt = DateTime.UtcNow;

        _unitOfWork.LeaveRequests.Update(request);
        await _unitOfWork.SaveChangesAsync(ct);

        if (request.Employee.UserId is { } employeeUserId)
        {
            var statusText = newStatus == LeaveRequestStatus.Approved ? "تمت الموافقة على" : "تم رفض";
            await _notificationService.NotifyUserAsync(employeeUserId,
                "تحديث طلب الإجازة",
                $"{statusText} طلب إجازتك من {request.StartDate:yyyy-MM-dd} إلى {request.EndDate:yyyy-MM-dd}.",
                newStatus == LeaveRequestStatus.Approved ? NotificationType.Success : NotificationType.Warning,
                null, ct);
        }

        return await GetByIdAsync(request.Id, ct);
    }

    public async Task<Result<LeaveRequestResponse>> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var request = await _unitOfWork.LeaveRequests.GetByIdAsync(id, ct);
        if (request is null)
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NotFound);

        if (request.Status != LeaveRequestStatus.Pending)
            return Result.Failure<LeaveRequestResponse>(LeaveRequestErrors.NotPending);

        request.Status = LeaveRequestStatus.Cancelled;
        _unitOfWork.LeaveRequests.Update(request);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(request.Id, ct);
    }
    public async Task<Result<List<LeaveRequestResponse>>> GetMyRequestsAsync(CancellationToken ct = default)
    {
        var employee = await _unitOfWork.Employees.Query().FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId, ct);
        if (employee is null)
            return Result.Failure<List<LeaveRequestResponse>>(LeaveRequestErrors.NoLinkedEmployee);

        var requests = await _unitOfWork.LeaveRequests.Query()
            .Include(l => l.Employee)
            .Where(l => l.EmployeeId == employee.Id)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(ct);

        return Result.Success(requests.Select(ToResponse).ToList());
    }
    private static LeaveRequestResponse ToResponse(LeaveRequest l) => new(
        l.Id, l.EmployeeId, l.Employee.FullName, l.LeaveType.ToString(), l.StartDate, l.EndDate, l.Reason, l.Status.ToString(), l.ApprovedAt);
}