public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    string FullName,
    Guid CompanyId,
    List<string> Roles);