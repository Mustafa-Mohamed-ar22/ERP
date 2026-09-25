public interface IAccountService
{
    Task<Result<List<AccountResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<AccountBalanceResponse>> GetBalanceAsync(Guid id, CancellationToken ct = default);
    Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request, CancellationToken ct = default);
    Task<Result<AccountResponse>> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<TrialBalanceResponse>> GetTrialBalanceAsync(CancellationToken ct = default);

    Task<Result<BalanceSheetResponse>> GetBalanceSheetAsync(DateTime asOfDate, CancellationToken ct = default);
    Task<Result<IncomeStatementResponse>> GetIncomeStatementAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
    Task<Result<CashFlowSummaryResponse>> GetCashFlowSummaryAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
}
