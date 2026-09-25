using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AccountService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AccountResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var accounts = await _unitOfWork.Accounts.Query().OrderBy(a => a.Code).ToListAsync(ct);
        return Result.Success(accounts.Select(ToResponse).ToList());
    }

    public async Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, ct);
        if (account is null)
            return Result.Failure<AccountResponse>(AccountErrors.NotFound);

        return Result.Success(ToResponse(account));
    }

    public async Task<Result<AccountBalanceResponse>> GetBalanceAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, ct);
        if (account is null)
            return Result.Failure<AccountBalanceResponse>(AccountErrors.NotFound);

        var lines = await _unitOfWork.JournalEntries.Query()
            .Where(j => j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .Where(l => l.AccountId == id)
            .ToListAsync(ct);

        var totalDebit = lines.Sum(l => l.Debit);
        var totalCredit = lines.Sum(l => l.Credit);

        // Debit-normal accounts (Asset/Expense) increase with debits; the rest increase with credits.
        var balance = account.AccountType is AccountType.Asset or AccountType.Expense
            ? totalDebit - totalCredit
            : totalCredit - totalDebit;

        return Result.Success(new AccountBalanceResponse(account.Id, account.Code, account.Name, totalDebit, totalCredit, balance));
    }

    public async Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<AccountType>(request.AccountType, true, out var accountType))
            return Result.Failure<AccountResponse>(new Error(
                "Account.InvalidType", "Invalid account type", "نوع الحساب غير صالح", StatusCodes.Status400BadRequest));

        var duplicateExists = await _unitOfWork.Accounts.Query().AnyAsync(a => a.Code == request.Code, ct);
        if (duplicateExists)
            return Result.Failure<AccountResponse>(AccountErrors.DuplicateCode);

        if (request.ParentAccountId is { } parentId)
        {
            var parent = await _unitOfWork.Accounts.GetByIdAsync(parentId, ct);
            if (parent is null)
                return Result.Failure<AccountResponse>(AccountErrors.ParentNotFound);

            if (parent.AccountType != accountType)
                return Result.Failure<AccountResponse>(AccountErrors.ParentTypeMismatch);
        }

        var account = new Account
        {
            CompanyId = _currentUser.CompanyId,
            Code = request.Code,
            Name = request.Name,
            AccountType = accountType,
            ParentAccountId = request.ParentAccountId,
            IsActive = true
        };

        await _unitOfWork.Accounts.AddAsync(account, ct);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<AccountResponse>(AccountErrors.DuplicateCode);
        }
        return Result.Success(ToResponse(account));
    }

    public async Task<Result<AccountResponse>> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, ct);
        if (account is null)
            return Result.Failure<AccountResponse>(AccountErrors.NotFound);

        var duplicateExists = await _unitOfWork.Accounts.Query()
            .AnyAsync(a => a.Code == request.Code && a.Id != id, ct);
        if (duplicateExists)
            return Result.Failure<AccountResponse>(AccountErrors.DuplicateCode);

        // Intentionally NOT allowing AccountType/ParentAccountId changes here — reclassifying an account
        // that may already have posted transactions is a much bigger operation (would need to migrate
        // historical balances); only Code/Name/IsActive are safe to freely edit post-creation.
        account.Code = request.Code;
        account.Name = request.Name;
        account.IsActive = request.IsActive;

        _unitOfWork.Accounts.Update(account);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<AccountResponse>(WarehouseErrors.DuplicateCode);
        }
        return Result.Success(ToResponse(account));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, ct);
        if (account is null)
            return Result.Failure(AccountErrors.NotFound);

        var hasChildren = await _unitOfWork.Accounts.Query().AnyAsync(a => a.ParentAccountId == id, ct);
        var hasTransactions = await _unitOfWork.JournalEntries.Query()
            .SelectMany(j => j.Lines)
            .AnyAsync(l => l.AccountId == id, ct);

        if (hasChildren || hasTransactions)
            return Result.Failure(AccountErrors.HasChildrenOrTransactions);

        account.IsDeleted = true;
        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
    public async Task<Result<TrialBalanceResponse>> GetTrialBalanceAsync(CancellationToken ct = default)
    {
        var accounts = await _unitOfWork.Accounts.Query().OrderBy(a => a.Code).ToListAsync(ct);

        // Group in SQL rather than pulling every posted line into memory — scales better as journal history grows.
        var totalsByAccount = await _unitOfWork.JournalEntries.Query()
            .Where(j => j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .GroupBy(l => l.AccountId)
            .Select(g => new { AccountId = g.Key, TotalDebit = g.Sum(l => l.Debit), TotalCredit = g.Sum(l => l.Credit) })
            .ToDictionaryAsync(g => g.AccountId, g => g, ct);

        var lines = new List<TrialBalanceLineResponse>();
        foreach (var account in accounts)
        {
            totalsByAccount.TryGetValue(account.Id, out var totals);
            var totalDebit = totals?.TotalDebit ?? 0;
            var totalCredit = totals?.TotalCredit ?? 0;

            var isDebitNormal = account.AccountType is AccountType.Asset or AccountType.Expense;
            var balance = isDebitNormal ? totalDebit - totalCredit : totalCredit - totalDebit;

            decimal debitBalance, creditBalance;
            if (isDebitNormal)
            {
                debitBalance = balance >= 0 ? balance : 0;
                creditBalance = balance < 0 ? -balance : 0;
            }
            else
            {
                creditBalance = balance >= 0 ? balance : 0;
                debitBalance = balance < 0 ? -balance : 0;
            }

            lines.Add(new TrialBalanceLineResponse(
                account.Id, account.Code, account.Name, account.AccountType.ToString(),
                totalDebit, totalCredit, debitBalance, creditBalance));
        }

        var totalDebitBalances = lines.Sum(l => l.DebitBalance);
        var totalCreditBalances = lines.Sum(l => l.CreditBalance);

        return Result.Success(new TrialBalanceResponse(lines, totalDebitBalances, totalCreditBalances, totalDebitBalances == totalCreditBalances));
    }
    public async Task<Result<BalanceSheetResponse>> GetBalanceSheetAsync(DateTime asOfDate, CancellationToken ct = default)
    {
        var accounts = await _unitOfWork.Accounts.Query().OrderBy(a => a.Code).ToListAsync(ct);

        // Same GroupBy-in-SQL pattern as GetTrialBalanceAsync, with the addition of the date cutoff —
        // this is the key difference from Trial Balance, which is all-time cumulative with no date filter.
        var totalsByAccount = await _unitOfWork.JournalEntries.Query()
            .Where(j => j.Status == JournalEntryStatus.Posted && j.EntryDate <= asOfDate)
            .SelectMany(j => j.Lines)
            .GroupBy(l => l.AccountId)
            .Select(g => new { AccountId = g.Key, TotalDebit = g.Sum(l => l.Debit), TotalCredit = g.Sum(l => l.Credit) })
            .ToDictionaryAsync(g => g.AccountId, g => g, ct);

        decimal GetBalance(Account account)
        {
            totalsByAccount.TryGetValue(account.Id, out var totals);
            var debit = totals?.TotalDebit ?? 0;
            var credit = totals?.TotalCredit ?? 0;
            var isDebitNormal = account.AccountType is AccountType.Asset or AccountType.Expense;
            return isDebitNormal ? debit - credit : credit - debit;
        }

        var assets = accounts.Where(a => a.AccountType == AccountType.Asset)
            .Select(a => new FinancialStatementLineResponse(a.Id, a.Code, a.Name, GetBalance(a))).ToList();
        var liabilities = accounts.Where(a => a.AccountType == AccountType.Liability)
            .Select(a => new FinancialStatementLineResponse(a.Id, a.Code, a.Name, GetBalance(a))).ToList();
        var equity = accounts.Where(a => a.AccountType == AccountType.Equity)
            .Select(a => new FinancialStatementLineResponse(a.Id, a.Code, a.Name, GetBalance(a))).ToList();

        var totalAssets = assets.Sum(l => l.Balance);
        var totalLiabilities = liabilities.Sum(l => l.Balance);
        var totalEquityAccounts = equity.Sum(l => l.Balance);

        // No formal period-closing process exists, so Net Income since inception is folded into Equity live,
        // as a computed "Current Earnings" line rather than requiring an actual closing entry — the pragmatic
        // MVP approach discussed before building this.
        var totalRevenue = accounts.Where(a => a.AccountType == AccountType.Revenue).Sum(GetBalance);
        var totalExpenses = accounts.Where(a => a.AccountType == AccountType.Expense).Sum(GetBalance);
        var currentEarnings = totalRevenue - totalExpenses;
        var totalEquity = totalEquityAccounts + currentEarnings;

        return Result.Success(new BalanceSheetResponse(
            asOfDate, assets, totalAssets, liabilities, totalLiabilities, equity, currentEarnings, totalEquity,
            totalAssets == totalLiabilities + totalEquity));
    }
    public async Task<Result<IncomeStatementResponse>> GetIncomeStatementAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        if (periodEnd < periodStart)
            return Result.Failure<IncomeStatementResponse>(AccountErrors.InvalidPeriod);

        var accounts = await _unitOfWork.Accounts.Query()
            .Where(a => a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense)
            .OrderBy(a => a.Code).ToListAsync(ct);

        var totalsByAccount = await _unitOfWork.JournalEntries.Query()
            .Where(j => j.Status == JournalEntryStatus.Posted && j.EntryDate >= periodStart && j.EntryDate <= periodEnd)
            .SelectMany(j => j.Lines)
            .GroupBy(l => l.AccountId)
            .Select(g => new { AccountId = g.Key, TotalDebit = g.Sum(l => l.Debit), TotalCredit = g.Sum(l => l.Credit) })
            .ToDictionaryAsync(g => g.AccountId, g => g, ct);

        decimal GetBalance(Account account)
        {
            totalsByAccount.TryGetValue(account.Id, out var totals);
            var debit = totals?.TotalDebit ?? 0;
            var credit = totals?.TotalCredit ?? 0;
            return account.AccountType == AccountType.Revenue ? credit - debit : debit - credit;
        }

        var revenues = accounts.Where(a => a.AccountType == AccountType.Revenue)
            .Select(a => new FinancialStatementLineResponse(a.Id, a.Code, a.Name, GetBalance(a))).ToList();
        var expenses = accounts.Where(a => a.AccountType == AccountType.Expense)
            .Select(a => new FinancialStatementLineResponse(a.Id, a.Code, a.Name, GetBalance(a))).ToList();

        var totalRevenue = revenues.Sum(l => l.Balance);
        var totalExpenses = expenses.Sum(l => l.Balance);

        return Result.Success(new IncomeStatementResponse(
            periodStart, periodEnd, revenues, totalRevenue, expenses, totalExpenses, totalRevenue - totalExpenses));
    }
    public async Task<Result<CashFlowSummaryResponse>> GetCashFlowSummaryAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        if (periodEnd < periodStart)
            return Result.Failure<CashFlowSummaryResponse>(AccountErrors.InvalidPeriod);

        var settings = await _unitOfWork.AccountingSettings.Query().FirstOrDefaultAsync(ct);
        if (settings?.CashAccountId is null)
            return Result.Failure<CashFlowSummaryResponse>(AccountErrors.CashAccountNotConfigured);

        var cashAccountId = settings.CashAccountId.Value;

        var openingTotals = await _unitOfWork.JournalEntries.Query()
            .Where(j => j.Status == JournalEntryStatus.Posted && j.EntryDate < periodStart)
            .SelectMany(j => j.Lines)
            .Where(l => l.AccountId == cashAccountId)
            .GroupBy(l => 1)
            .Select(g => new { Debit = g.Sum(l => l.Debit), Credit = g.Sum(l => l.Credit) })
            .FirstOrDefaultAsync(ct);

        var openingBalance = (openingTotals?.Debit ?? 0) - (openingTotals?.Credit ?? 0); // Cash is Asset — debit-normal

        var periodLines = await _unitOfWork.JournalEntries.Query()
            .Where(j => j.Status == JournalEntryStatus.Posted && j.EntryDate >= periodStart && j.EntryDate <= periodEnd)
            .SelectMany(j => j.Lines.Select(l => new { j.EntryDate, l.Description, l.Debit, l.Credit, l.AccountId }))
            .Where(x => x.AccountId == cashAccountId)
            .OrderBy(x => x.EntryDate)
            .ToListAsync(ct);

        var movements = new List<CashFlowLineResponse>();
        var runningBalance = openingBalance;
        foreach (var line in periodLines)
        {
            runningBalance += line.Debit - line.Credit;
            movements.Add(new CashFlowLineResponse(line.EntryDate, line.Description ?? string.Empty, line.Debit, line.Credit, runningBalance));
        }

        var closingBalance = runningBalance;

        return Result.Success(new CashFlowSummaryResponse(
            periodStart, periodEnd, openingBalance, closingBalance, closingBalance - openingBalance, movements));
    }
    private static AccountResponse ToResponse(Account account) => new(
        account.Id, account.Code, account.Name, account.AccountType.ToString(), account.ParentAccountId, account.IsActive);
}