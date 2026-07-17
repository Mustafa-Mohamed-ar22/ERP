public class Department : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}