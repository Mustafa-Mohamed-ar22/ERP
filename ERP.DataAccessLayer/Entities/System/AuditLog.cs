// Entities/System/Setting.cs
// Entities/System/AuditLog.cs
public class AuditLog : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = default!;    // Create/Update/Delete
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string? OldValues { get; set; }             // JSON
    public string? NewValues { get; set; }             // JSON
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
