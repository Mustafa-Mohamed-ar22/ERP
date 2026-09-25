public class CashierShiftCashMovement : BaseEntity
{
    public Guid CashierShiftId { get; set; }
    public CashierShift CashierShift { get; set; } = default!;
    public CashMovementType MovementType { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = default!;
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }
}
