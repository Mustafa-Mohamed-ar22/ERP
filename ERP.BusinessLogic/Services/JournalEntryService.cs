using Microsoft.EntityFrameworkCore;
public class JournalEntryService : IJournalEntryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public JournalEntryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<JournalEntryResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var entries = await _unitOfWork.JournalEntries.Query()
            .Include(j => j.Lines).ThenInclude(l => l.Account)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync(ct);

        return Result.Success(entries.Select(ToResponse).ToList());
    }

    public async Task<Result<JournalEntryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _unitOfWork.JournalEntries.Query()
            .Include(j => j.Lines).ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        if (entry is null)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.NotFound);

        return Result.Success(ToResponse(entry));
    }

    public async Task<Result<JournalEntryResponse>> CreateAsync(CreateJournalEntryRequest request, CancellationToken ct = default)
    {
        if (request.Lines is null || request.Lines.Count < 2)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.EmptyLines);

        var totalDebit = request.Lines.Sum(l => l.Debit);
        var totalCredit = request.Lines.Sum(l => l.Credit);
        if (totalDebit != totalCredit || totalDebit == 0)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.Unbalanced);

        var accountIds = request.Lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _unitOfWork.Accounts.Query().Where(a => accountIds.Contains(a.Id)).ToListAsync(ct);
        if (accounts.Count != accountIds.Count)
            return Result.Failure<JournalEntryResponse>(AccountErrors.NotFound);

        // Reject posting to any account that has sub-accounts — only leaf accounts should carry transactions
        var accountsWithChildren = await _unitOfWork.Accounts.Query()
            .Where(a => a.ParentAccountId != null && accountIds.Contains(a.ParentAccountId.Value))
            .Select(a => a.ParentAccountId!.Value)
            .Distinct()
            .ToListAsync(ct);

        if (accountsWithChildren.Count > 0)
            return Result.Failure<JournalEntryResponse>(AccountErrors.NotPostable);

        var entryCount = await _unitOfWork.JournalEntries.Query().CountAsync(ct);
        var entry = new JournalEntry
        {
            CompanyId = _currentUser.CompanyId,
            EntryNumber = $"JE-{entryCount + 1:D6}",
            EntryDate = request.EntryDate,
            Description = request.Description,
            Status = JournalEntryStatus.Draft,
            Lines = request.Lines.Select(l => new JournalEntryLine
            {
                AccountId = l.AccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        };

        await _unitOfWork.JournalEntries.AddAsync(entry, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entry.Id, ct);
    }

    public async Task<Result<JournalEntryResponse>> PostAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _unitOfWork.JournalEntries.Query()
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        if (entry is null)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.NotFound);

        if (entry.Status == JournalEntryStatus.Posted)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.AlreadyPosted);

        var totalDebit = entry.Lines.Sum(l => l.Debit);
        var totalCredit = entry.Lines.Sum(l => l.Credit);
        if (totalDebit != totalCredit || totalDebit == 0)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.Unbalanced);

        entry.Status = JournalEntryStatus.Posted;
        entry.PostedAt = DateTime.UtcNow;
        entry.PostedBy = _currentUser.UserId;

        _unitOfWork.JournalEntries.Update(entry);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entry.Id, ct);
    }

    public async Task<Result<JournalEntryResponse>> ReverseAsync(Guid id, CancellationToken ct = default)
    {
        var original = await _unitOfWork.JournalEntries.Query()
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        if (original is null)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.NotFound);

        if (original.Status != JournalEntryStatus.Posted)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.NotPosted);

        var alreadyReversed = await _unitOfWork.JournalEntries.Query().AnyAsync(j => j.ReversalOfEntryId == id, ct);
        if (alreadyReversed)
            return Result.Failure<JournalEntryResponse>(JournalEntryErrors.AlreadyReversed);

        var entryCount = await _unitOfWork.JournalEntries.Query().CountAsync(ct);
        var reversal = new JournalEntry
        {
            CompanyId = _currentUser.CompanyId,
            EntryNumber = $"JE-{entryCount + 1:D6}",
            EntryDate = DateTime.UtcNow,
            Description = $"Reversal of {original.EntryNumber}",
            Status = JournalEntryStatus.Posted,
            PostedAt = DateTime.UtcNow,
            PostedBy = _currentUser.UserId,
            ReversalOfEntryId = original.Id,
            Lines = original.Lines.Select(l => new JournalEntryLine
            {
                AccountId = l.AccountId,
                Debit = l.Credit,   // swapped — cancels the original's GL effect
                Credit = l.Debit,
                Description = $"Reversal: {l.Description}"
            }).ToList()
        };

        await _unitOfWork.JournalEntries.AddAsync(reversal, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(reversal.Id, ct);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id, ct);
        if (entry is null)
            return Result.Failure(JournalEntryErrors.NotFound);

        if (entry.Status != JournalEntryStatus.Draft)
            return Result.Failure(JournalEntryErrors.NotDraft);

        _unitOfWork.JournalEntries.Remove(entry); // hard delete OK — still draft, never affected the GL
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static JournalEntryResponse ToResponse(JournalEntry entry) => new(
        entry.Id, entry.EntryNumber, entry.EntryDate, entry.Description, entry.Status.ToString(), entry.ReversalOfEntryId,
        entry.Lines.Select(l => new JournalEntryLineResponse(
            l.Id, l.AccountId, l.Account.Code, l.Account.Name, l.Debit, l.Credit, l.Description)).ToList());
}