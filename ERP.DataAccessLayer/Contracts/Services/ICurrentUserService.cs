public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid CompanyId { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}