public class Company : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegisterNumber { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}