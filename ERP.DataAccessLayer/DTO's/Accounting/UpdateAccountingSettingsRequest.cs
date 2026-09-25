public record UpdateAccountingSettingsRequest(
    Guid? InventoryAccountId, Guid? AccountsPayableAccountId, Guid? AccountsReceivableAccountId,
    Guid? RevenueAccountId, Guid? CostOfGoodsSoldAccountId, Guid? CashAccountId);