using Microsoft.EntityFrameworkCore;

public class AccountingSettingsService : IAccountingSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AccountingSettingsService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<AccountingSettingsResponse>> GetAsync(CancellationToken ct = default)
    {
        var settings = await GetOrCreateAsync(ct);
        return Result.Success(ToResponse(settings));
    }

    public async Task<Result<AccountingSettingsResponse>> UpdateAsync(UpdateAccountingSettingsRequest request, CancellationToken ct = default)
    {
        var expectedTypes = new (Guid? Id, AccountType Expected)[]
        {
            (request.InventoryAccountId, AccountType.Asset),
            (request.AccountsPayableAccountId, AccountType.Liability),
            (request.AccountsReceivableAccountId, AccountType.Asset),
            (request.RevenueAccountId, AccountType.Revenue),
            (request.CostOfGoodsSoldAccountId, AccountType.Expense)
        };

        foreach (var (id, expectedType) in expectedTypes)
        {
            if (id is null) continue;

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (account is null)
                return Result.Failure<AccountingSettingsResponse>(AccountErrors.NotFound);

            if (account.AccountType != expectedType)
                return Result.Failure<AccountingSettingsResponse>(AccountingSettingsErrors.AccountTypeMismatch);
        }

        var settings = await GetOrCreateAsync(ct);
        settings.InventoryAccountId = request.InventoryAccountId;
        settings.AccountsPayableAccountId = request.AccountsPayableAccountId;
        settings.AccountsReceivableAccountId = request.AccountsReceivableAccountId;
        settings.RevenueAccountId = request.RevenueAccountId;
        settings.CostOfGoodsSoldAccountId = request.CostOfGoodsSoldAccountId;

        await _context.SaveChangesAsync(ct);

        return Result.Success(ToResponse(settings));
    }

    private async Task<AccountingSettings> GetOrCreateAsync(CancellationToken ct)
    {
        var settings = await _context.AccountingSettings
            .FirstOrDefaultAsync(s => s.CompanyId == _currentUser.CompanyId, ct);

        if (settings is null)
        {
            settings = new AccountingSettings { CompanyId = _currentUser.CompanyId };
            _context.AccountingSettings.Add(settings);
            await _context.SaveChangesAsync(ct);
        }

        return settings;
    }

    private static AccountingSettingsResponse ToResponse(AccountingSettings s) => new(
        s.InventoryAccountId, s.AccountsPayableAccountId, s.AccountsReceivableAccountId,
        s.RevenueAccountId, s.CostOfGoodsSoldAccountId);
}