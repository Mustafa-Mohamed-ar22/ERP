using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<Guid>   // no longer : ITenantEntity
{
    public Guid CompanyId { get; set; }      // property stays — just not used for auto-filtering anymore
    public Company Company { get; set; } = default!;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();
}