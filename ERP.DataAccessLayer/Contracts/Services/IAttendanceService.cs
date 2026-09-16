public interface IAttendanceService
{
    Task<Result<List<AttendanceRecordResponse>>> GetAllAsync(Guid? employeeId, CancellationToken ct = default);
    Task<Result<AttendanceRecordResponse>> CheckInAsync(CancellationToken ct = default);
    Task<Result<AttendanceRecordResponse>> CheckOutAsync(CancellationToken ct = default);
    Task<Result<List<AttendanceRecordResponse>>> GetMyHistoryAsync(CancellationToken ct = default);

}