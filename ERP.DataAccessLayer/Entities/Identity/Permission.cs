public class Permission : BaseEntity
{
    public string Code { get; set; } = default!;      // unique, e.g. "hr.employees.create"
    public string Module { get; set; } = default!;    // e.g. "HR", "Sales", "Core"
    public string Description { get; set; } = default!;
}