using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class CashierShiftService : ICashierShiftService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly INotificationService _notificationService;
    public CashierShiftService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INumberSequenceService numberSequenceService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _numberSequenceService = numberSequenceService;
        _notificationService = notificationService;
    }

    public async Task<Result<CashierShiftResponse>> OpenShiftAsync(OpenShiftRequest request, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse is null) return Result.Failure<CashierShiftResponse>(CashierErrors.WarehouseNotFound);

        var alreadyOpen = await _unitOfWork.CashierShifts.Query()
            .AnyAsync(s => s.WarehouseId == request.WarehouseId && s.CashierUserId == _currentUser.UserId && s.Status == CashierShiftStatus.Open, ct);
        if (alreadyOpen) return Result.Failure<CashierShiftResponse>(CashierErrors.ShiftAlreadyOpen);

        var shiftNumber = await _numberSequenceService.GetNextNumberAsync("CashierShift", "SH", 6, ct);

        var shift = new CashierShift
        {
            CompanyId = _currentUser.CompanyId,
            WarehouseId = request.WarehouseId,
            CashierUserId = _currentUser.UserId,
            ShiftNumber = shiftNumber,
            OpenedAt = DateTime.UtcNow,
            OpeningCashBalance = request.OpeningCashBalance,
            Status = CashierShiftStatus.Open,
            OpenNotes = request.Notes
        };

        try
        {
            await _unitOfWork.CashierShifts.AddAsync(shift, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // race: two concurrent open-shift calls for the same cashier+warehouse
            return Result.Failure<CashierShiftResponse>(CashierErrors.ShiftAlreadyOpen);
        }

        return Result.Success(ToResponse(shift, warehouse.Name));
    }

    public async Task<Result> AddCashMovementAsync(Guid shiftId, CashMovementRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<CashMovementType>(request.MovementType, true, out var movementType))
            return Result.Failure(CashierErrors.InvalidMovementType);

        var shift = await _unitOfWork.CashierShifts.GetByIdAsync(shiftId, ct);
        if (shift is null) return Result.Failure(CashierErrors.ShiftNotFound);
        if (shift.Status == CashierShiftStatus.Closed) return Result.Failure(CashierErrors.ShiftAlreadyClosed);

        var movement = new CashierShiftCashMovement
        {
            CashierShiftId = shiftId,
            MovementType = movementType,
            Amount = request.Amount,
            Reason = request.Reason,
            MovementDate = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.CashierShiftCashMovements.AddAsync(movement, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<ShiftClosingReportResponse>> CloseShiftAsync(Guid shiftId, CloseShiftRequest request, CancellationToken ct = default)
    {
        var shift = await _unitOfWork.CashierShifts.Query()
            .Include(s => s.Orders).ThenInclude(o => o.Payments)
            .Include(s => s.CashMovements)
            .FirstOrDefaultAsync(s => s.Id == shiftId, ct);

        if (shift is null) return Result.Failure<ShiftClosingReportResponse>(CashierErrors.ShiftNotFound);
        if (shift.Status == CashierShiftStatus.Closed) return Result.Failure<ShiftClosingReportResponse>(CashierErrors.ShiftAlreadyClosed);

        var report = BuildReportCore(shift, request.CountedClosingCash);

        shift.Status = CashierShiftStatus.Closed;
        shift.ClosedAt = DateTime.UtcNow;
        shift.ExpectedClosingCash = report.ExpectedClosingCash;
        shift.CountedClosingCash = request.CountedClosingCash;
        shift.DiscrepancyAmount = report.DiscrepancyAmount;
        shift.CloseNotes = request.Notes;

        _unitOfWork.CashierShifts.Update(shift);
        await _unitOfWork.SaveChangesAsync(ct);
        if (report.DiscrepancyAmount is { } discrepancy /*&& Math.Abs(discrepancy) >= 50m*/)
        {
            await _notificationService.NotifyUsersWithPermissionAsync("cashier.shifts.view",
                "تباين في تقفيل الوردية",
                $"الوردية {shift.ShiftNumber} أُغلقت بفارق {discrepancy:0.00} في النقدية.",
                NotificationType.Warning, null, ct);
        }
        return Result.Success(report);
    }

    public async Task<Result<ShiftClosingReportResponse>> GetClosingReportAsync(Guid shiftId, CancellationToken ct = default)
    {
        var shift = await _unitOfWork.CashierShifts.Query()
            .Include(s => s.Orders).ThenInclude(o => o.Payments)
            .Include(s => s.CashMovements)
            .FirstOrDefaultAsync(s => s.Id == shiftId, ct);

        if (shift is null) return Result.Failure<ShiftClosingReportResponse>(CashierErrors.ShiftNotFound);

        // Preview mode: shows expected cash only (no counted figure yet if still open)
        return Result.Success(BuildReportCore(shift, shift.CountedClosingCash));
    }

    public async Task<Result<CashierShiftResponse>> GetMyCurrentShiftAsync(CancellationToken ct = default)
    {
        var shift = await _unitOfWork.CashierShifts.Query()
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.CashierUserId == _currentUser.UserId && s.Status == CashierShiftStatus.Open, ct);

        return shift is null
            ? Result.Failure<CashierShiftResponse>(CashierErrors.NoOpenShift)
            : Result.Success(ToResponse(shift, shift.Warehouse.Name));
    }

    public async Task<Result<List<CashierShiftResponse>>> GetMyHistoryAsync(CancellationToken ct = default)
    {
        var shifts = await _unitOfWork.CashierShifts.Query()
            .Include(s => s.Warehouse)
            .Where(s => s.CashierUserId == _currentUser.UserId)
            .OrderByDescending(s => s.OpenedAt)
            .ToListAsync(ct);

        return Result.Success(shifts.Select(s => ToResponse(s, s.Warehouse.Name)).ToList());
    }

    public async Task<Result<CashierShiftResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var shift = await _unitOfWork.CashierShifts.Query()
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return shift is null
            ? Result.Failure<CashierShiftResponse>(CashierErrors.ShiftNotFound)
            : Result.Success(ToResponse(shift, shift.Warehouse.Name));
    }

    private static ShiftClosingReportResponse BuildReportCore(CashierShift shift, decimal? countedCash)
    {
        var completedOrders = shift.Orders.Where(o => o.Status == CashierOrderStatus.Completed).ToList();
        var voidedCount = shift.Orders.Count(o => o.Status == CashierOrderStatus.Voided);

        var allPayments = completedOrders.SelectMany(o => o.Payments).ToList();
        var cashSalesGross = allPayments.Where(p => p.Method == CashierPaymentMethod.Cash).Sum(p => p.Amount);
        var cardSales = allPayments.Where(p => p.Method == CashierPaymentMethod.Card).Sum(p => p.Amount);
        var otherSales = allPayments.Where(p => p.Method == CashierPaymentMethod.Other).Sum(p => p.Amount);

        // Change given on cash tenders reduces what actually stays in the drawer.
        // (Computed once, correctly — the earlier draft computed this twice and discarded the first result.)
        var changeGiven = completedOrders.Sum(o =>
        {
            var nonCash = o.Payments.Where(p => p.Method != CashierPaymentMethod.Cash).Sum(p => p.Amount);
            var cashTendered = o.Payments.Where(p => p.Method == CashierPaymentMethod.Cash).Sum(p => p.Amount);
            var cashOwed = o.TotalAmount - nonCash;
            return Math.Max(0, cashTendered - cashOwed);
        });

        var netCashSales = cashSalesGross - changeGiven;
        var cashIn = shift.CashMovements.Where(m => m.MovementType == CashMovementType.CashIn).Sum(m => m.Amount);
        var cashOut = shift.CashMovements.Where(m => m.MovementType == CashMovementType.CashOut).Sum(m => m.Amount);
        var expected = shift.OpeningCashBalance + netCashSales + cashIn - cashOut;

        return new ShiftClosingReportResponse(
            shift.Id, shift.ShiftNumber, shift.OpenedAt, shift.ClosedAt,
            shift.OpeningCashBalance, completedOrders.Count, voidedCount,
            netCashSales, cardSales, otherSales, cashIn, cashOut, expected,
            countedCash, countedCash.HasValue ? countedCash.Value - expected : null);
    }

    private static CashierShiftResponse ToResponse(CashierShift s, string warehouseName) => new(
        s.Id, s.ShiftNumber, s.WarehouseId, warehouseName, s.CashierUserId, s.OpenedAt, s.ClosedAt,
        s.OpeningCashBalance, s.ExpectedClosingCash, s.CountedClosingCash, s.DiscrepancyAmount, s.Status.ToString());
}