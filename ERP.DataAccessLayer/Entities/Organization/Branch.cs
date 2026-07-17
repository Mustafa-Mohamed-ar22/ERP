public class Branch : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsMain { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
}