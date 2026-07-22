public class RefreshToken
{
    public string Token { get; set; } = default!;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedIn { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsActive => RevokedIn is null && !IsExpired;
}