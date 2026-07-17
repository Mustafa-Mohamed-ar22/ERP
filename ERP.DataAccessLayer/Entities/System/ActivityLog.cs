public class ActivityLog : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = default!;
    public string Module { get; set; } = default!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
