public class AccountingSettings : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid? InventoryAccountId { get; set; }
    public Account? InventoryAccount { get; set; }
    public Guid? AccountsPayableAccountId { get; set; }
    public Account? AccountsPayableAccount { get; set; }
    public Guid? AccountsReceivableAccountId { get; set; }
    public Account? AccountsReceivableAccount { get; set; }
    public Guid? RevenueAccountId { get; set; }
    public Account? RevenueAccount { get; set; }
    public Guid? CostOfGoodsSoldAccountId { get; set; }
    public Account? CostOfGoodsSoldAccount { get; set; }
}