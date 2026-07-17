// Entities/System/Setting.cs
public class Setting : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Key { get; set; } = default!;
    public string? Value { get; set; }
    public string Category { get; set; } = default!;  // "General", "Email", "SMS", "AI" ...
}
