using Microsoft.EntityFrameworkCore;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AttendanceService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AttendanceRecordResponse>>> GetAllAsync(Guid? employeeId, CancellationToken ct = default)
    {
        var query = _unitOfWork.AttendanceRecords.Query().Include(a => a.Employee).AsQueryable();
        if (employeeId is { } id)
            query = query.Where(a => a.EmployeeId == id);

        var records = await query.OrderByDescending(a => a.Date).ToListAsync(ct);
        return Result.Success(records.Select(ToResponse).ToList());
    }
    public async Task<Result<AttendanceRecordResponse>> CheckInAsync(CancellationToken ct = default)
    {
        var employee = await _unitOfWork.Employees.Query().FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId, ct);
        if (employee is null)
            return Result.Failure<AttendanceRecordResponse>(AttendanceErrors.NoLinkedEmployee);
        if (employee.Status != EmploymentStatus.Active)
            return Result.Failure<AttendanceRecordResponse>(AttendanceErrors.EmployeeNotActive);
        var today = DateTime.UtcNow.Date;
        var record = await _unitOfWork.AttendanceRecords.Query()
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today, ct);

        if (record?.CheckInTime is not null)
            return Result.Failure<AttendanceRecordResponse>(AttendanceErrors.AlreadyCheckedIn);

        if (record is null)
        {
            record = new AttendanceRecord
            {
                CompanyId = _currentUser.CompanyId,
                EmployeeId = employee.Id,
                Date = today,
                CheckInTime = DateTime.UtcNow,
                Status = AttendanceStatus.Present
            };
            await _unitOfWork.AttendanceRecords.AddAsync(record, ct);
        }
        else
        {
            record.CheckInTime = DateTime.UtcNow;
            _unitOfWork.AttendanceRecords.Update(record);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(record, employee.FullName));
    }

    public async Task<Result<AttendanceRecordResponse>> CheckOutAsync(CancellationToken ct = default)
    {
        var employee = await _unitOfWork.Employees.Query().FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId, ct);
        if (employee is null)
            return Result.Failure<AttendanceRecordResponse>(AttendanceErrors.NoLinkedEmployee);
        if (employee.Status != EmploymentStatus.Active)
            return Result.Failure<AttendanceRecordResponse>(AttendanceErrors.EmployeeNotActive);
        var today = DateTime.UtcNow.Date;
        var record = await _unitOfWork.AttendanceRecords.Query()
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today, ct);

        if (record?.CheckInTime is null)
            return Result.Failure<AttendanceRecordResponse>(AttendanceErrors.NotCheckedIn);

        if (record.CheckOutTime is not null)
            return Result.Failure<AttendanceRecordResponse>(AttendanceErrors.AlreadyCheckedOut);

        record.CheckOutTime = DateTime.UtcNow;
        _unitOfWork.AttendanceRecords.Update(record);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(record, employee.FullName));
    }
    public async Task<Result<List<AttendanceRecordResponse>>> GetMyHistoryAsync(CancellationToken ct = default)
    {
        var employee = await _unitOfWork.Employees.Query().FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId, ct);
        if (employee is null)
            return Result.Failure<List<AttendanceRecordResponse>>(AttendanceErrors.NoLinkedEmployee);

        var records = await _unitOfWork.AttendanceRecords.Query()
            .Where(a => a.EmployeeId == employee.Id)
            .OrderByDescending(a => a.Date)
            .ToListAsync(ct);

        return Result.Success(records.Select(a => ToResponse(a, employee.FullName)).ToList());
    }
    private static AttendanceRecordResponse ToResponse(AttendanceRecord a, string employeeName) => new(
        a.Id, a.EmployeeId, employeeName, a.Date, a.CheckInTime, a.CheckOutTime, a.Status.ToString(), a.Notes);

    private static AttendanceRecordResponse ToResponse(AttendanceRecord a) => ToResponse(a, a.Employee.FullName);
}