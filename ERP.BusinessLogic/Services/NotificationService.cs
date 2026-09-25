using Microsoft.EntityFrameworkCore;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context; // needed for the UserRoles/RolePermissions join in NotifyUsersWithPermissionAsync
    private readonly ICurrentUserService _currentUser;

    public NotificationService(IUnitOfWork unitOfWork, ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NotificationResponse>>> GetMyNotificationsAsync(bool? unreadOnly, CancellationToken ct = default)
    {
        var query = _unitOfWork.Notifications.Query().Where(n => n.UserId == _currentUser.UserId);
        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var notifications = await query.OrderByDescending(n => n.CreatedAt).ToListAsync(ct);
        return Result.Success(notifications.Select(ToResponse).ToList());
    }

    public async Task<Result<int>> GetMyUnreadCountAsync(CancellationToken ct = default)
    {
        var count = await _unitOfWork.Notifications.Query().CountAsync(n => n.UserId == _currentUser.UserId && !n.IsRead, ct);
        return Result.Success(count);
    }

    public async Task<Result> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _unitOfWork.Notifications.Query()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == _currentUser.UserId, ct);
        if (notification is null) return Result.Failure(NotificationErrors.NotFound);

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(CancellationToken ct = default)
    {
        var unread = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == _currentUser.UserId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
            n.IsRead = true;

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _unitOfWork.Notifications.Query()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == _currentUser.UserId, ct);
        if (notification is null) return Result.Failure(NotificationErrors.NotFound);

        _unitOfWork.Notifications.Remove(notification); // hard delete — not a financial/audit record
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task NotifyUserAsync(
        Guid userId, string title, string body, NotificationType type = NotificationType.Info, string? link = null, CancellationToken ct = default)
    {
        try
        {
            var notification = new Notification
            {
                CompanyId = _currentUser.CompanyId,
                UserId = userId,
                Title = title,
                Body = body,
                Type = type,
                IsRead = false,
                Link = link,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch
        {
            // Best-effort, fire-and-forget: a failed notification write must never break the business
            // action that triggered it — swallow here rather than propagating as a Result failure.
        }
    }

    public async Task NotifyUsersWithPermissionAsync(
        string permissionCode, string title, string body, NotificationType type = NotificationType.Info, string? link = null, CancellationToken ct = default)
    {
        try
        {
            var companyId = _currentUser.CompanyId;

            // Deliberately scoped to THIS company's roles only — without the CompanyId filter here, this
            // would notify users in other companies who happen to hold a role with the same permission code,
            // a real cross-tenant leak that's easy to miss since roles/permissions span the whole Permissions table.
            var userIds = await _context.UserRoles
                .Join(_context.Roles.Where(r => r.CompanyId == companyId), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Id })
                .Join(_context.RolePermissions.Where(rp => rp.Permission.Code == permissionCode), x => x.Id, rp => rp.RoleId, (x, rp) => x.UserId)
                .Distinct()
                .ToListAsync(ct);

            foreach (var userId in userIds)
                await NotifyUserAsync(userId, title, body, type, link, ct);
        }
        catch
        {
            // same best-effort reasoning
        }
    }

    private static NotificationResponse ToResponse(Notification n) => new(n.Id, n.Title, n.Body, n.Type.ToString(), n.IsRead, n.Link, n.CreatedAt);
}