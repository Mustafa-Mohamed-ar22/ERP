public interface INotificationService
{
    Task<Result<List<NotificationResponse>>> GetMyNotificationsAsync(bool? unreadOnly, CancellationToken ct = default);
    Task<Result<int>> GetMyUnreadCountAsync(CancellationToken ct = default);
    Task<Result> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);
    Task<Result> MarkAllAsReadAsync(CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid notificationId, CancellationToken ct = default);

    // Internal — called from other services, never exposed via a controller directly
    Task NotifyUserAsync(Guid userId, string title, string body, NotificationType type = NotificationType.Info, string? link = null, CancellationToken ct = default);
    Task NotifyUsersWithPermissionAsync(string permissionCode, string title, string body, NotificationType type = NotificationType.Info, string? link = null, CancellationToken ct = default);
}