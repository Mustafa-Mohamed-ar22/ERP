using Microsoft.EntityFrameworkCore;

public class NumberSequenceService : INumberSequenceService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NumberSequenceService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> GetNextNumberAsync(string documentType, string prefix, int padding, CancellationToken ct = default)
    {
        var companyId = _currentUser.CompanyId;

        // Atomic increment — SQL Server locks the row for the duration of the UPDATE, serializing concurrent callers.
        var updated = await _context.Database.SqlQuery<int>(
            $"UPDATE DocumentSequences SET LastNumber = LastNumber + 1 OUTPUT INSERTED.LastNumber WHERE CompanyId = {companyId} AND DocumentType = {documentType}"
        ).ToListAsync(ct);

        if (updated.Count > 0)
            return $"{prefix}-{updated[0].ToString().PadLeft(padding, '0')}";

        // No row yet for this (CompanyId, DocumentType) — first document of this type for this company.
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO DocumentSequences (Id, CompanyId, DocumentType, LastNumber) VALUES (NEWID(), {companyId}, {documentType}, 1)",
                ct);
            return $"{prefix}-{1.ToString().PadLeft(padding, '0')}";
        }
        catch (DbUpdateException)
        {
            // Another concurrent request created the row first (race on the INSERT itself) — retry the atomic update,
            // which will now succeed since the row exists.
            var retried = await _context.Database.SqlQuery<int>(
                $"UPDATE DocumentSequences SET LastNumber = LastNumber + 1 OUTPUT INSERTED.LastNumber WHERE CompanyId = {companyId} AND DocumentType = {documentType}"
            ).ToListAsync(ct);
            return $"{prefix}-{retried[0].ToString().PadLeft(padding, '0')}";
        }
    }
}