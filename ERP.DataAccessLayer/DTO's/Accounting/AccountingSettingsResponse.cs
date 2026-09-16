public record AccountingSettingsResponse(
    Guid? InventoryAccountId, Guid? AccountsPayableAccountId, Guid? AccountsReceivableAccountId,
    Guid? RevenueAccountId, Guid? CostOfGoodsSoldAccountId);
