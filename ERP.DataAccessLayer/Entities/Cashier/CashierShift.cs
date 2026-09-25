public class CashierShift : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid WarehouseId { get; set; }             // the till's stock-holding location — not Branch
    public Warehouse Warehouse { get; set; } = default!;
    public Guid CashierUserId { get; set; }            // ApplicationUser.Id — unenforced FK, same pattern as Employee.UserId
    public string ShiftNumber { get; set; } = default!;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningCashBalance { get; set; }
    public decimal? ExpectedClosingCash { get; set; }
    public decimal? CountedClosingCash { get; set; }
    public decimal? DiscrepancyAmount { get; set; }
    public CashierShiftStatus Status { get; set; } = CashierShiftStatus.Open;
    public string? OpenNotes { get; set; }
    public string? CloseNotes { get; set; }

    public ICollection<CashierOrder> Orders { get; set; } = new List<CashierOrder>();
    public ICollection<CashierShiftCashMovement> CashMovements { get; set; } = new List<CashierShiftCashMovement>();
}