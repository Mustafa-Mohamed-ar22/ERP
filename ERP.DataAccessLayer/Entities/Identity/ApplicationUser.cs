using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<Guid>, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}