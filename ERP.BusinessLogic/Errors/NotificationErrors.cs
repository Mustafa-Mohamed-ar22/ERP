using Microsoft.AspNetCore.Http;

public static class NotificationErrors
{
    public static readonly Error NotFound = new(
        "Notification.NotFound", "Notification not found", "الإشعار غير موجود", StatusCodes.Status404NotFound);
}