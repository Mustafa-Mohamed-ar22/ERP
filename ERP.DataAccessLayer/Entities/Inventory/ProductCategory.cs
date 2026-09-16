public class ProductCategory : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = default!;
    public Guid? ParentCategoryId { get; set; }
    public ProductCategory? ParentCategory { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductCategory> ChildCategories { get; set; } = new List<ProductCategory>();
}