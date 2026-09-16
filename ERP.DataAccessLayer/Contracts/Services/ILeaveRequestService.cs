public interface ILeaveRequestService
{
    Task<Result<List<LeaveRequestResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<LeaveRequestResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<LeaveRequestResponse>> CreateAsync(CreateLeaveRequestRequest request, CancellationToken ct = default);
    Task<Result<LeaveRequestResponse>> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<Result<LeaveRequestResponse>> RejectAsync(Guid id, CancellationToken ct = default);
    Task<Result<LeaveRequestResponse>> CancelAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<LeaveRequestResponse>>> GetMyRequestsAsync(CancellationToken ct = default);
}