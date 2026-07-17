// Entities/System/Setting.cs
// Entities/System/AuditLog.cs
// Entities/System/Notification.cs
// Entities/System/Notification.cs
public class Notification : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public string? Link { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
