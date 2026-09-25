public record NotificationResponse
    (Guid Id, string Title, string Body, string Type, bool IsRead, string? Link, DateTime CreatedAt);