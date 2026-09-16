public interface IAccountingSettingsService
{
    Task<Result<AccountingSettingsResponse>> GetAsync(CancellationToken ct = default);
    Task<Result<AccountingSettingsResponse>> UpdateAsync(UpdateAccountingSettingsRequest request, CancellationToken ct = default);
}