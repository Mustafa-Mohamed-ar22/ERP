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
        await _unitOfWork.SaveChangesAsync(ct);

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
        await _unitOfWork.SaveChangesAsync(ct);

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

    private static AccountResponse ToResponse(Account account) => new(
        account.Id, account.Code, account.Name, account.AccountType.ToString(), account.ParentAccountId, account.IsActive);
}