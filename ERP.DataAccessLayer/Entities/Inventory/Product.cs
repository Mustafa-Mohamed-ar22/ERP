public class Product : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = default!;
    public Guid? CategoryId { get; set; }              
    public ProductCategory? Category { get; set; }     
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsActive { get; set; } = true;
}
