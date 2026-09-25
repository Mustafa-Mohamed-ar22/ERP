public interface ICashierShiftService
{
    Task<Result<CashierShiftResponse>> OpenShiftAsync(OpenShiftRequest request, CancellationToken ct = default);
    Task<Result> AddCashMovementAsync(Guid shiftId, CashMovementRequest request, CancellationToken ct = default);
    Task<Result<ShiftClosingReportResponse>> CloseShiftAsync(Guid shiftId, CloseShiftRequest request, CancellationToken ct = default);
    Task<Result<ShiftClosingReportResponse>> GetClosingReportAsync(Guid shiftId, CancellationToken ct = default);
    Task<Result<CashierShiftResponse>> GetMyCurrentShiftAsync(CancellationToken ct = default);
    Task<Result<List<CashierShiftResponse>>> GetMyHistoryAsync(CancellationToken ct = default);
    Task<Result<CashierShiftResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
}
