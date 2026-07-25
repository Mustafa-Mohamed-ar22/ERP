public interface IJournalEntryService
{
    Task<Result<List<JournalEntryResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<JournalEntryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<JournalEntryResponse>> CreateAsync(CreateJournalEntryRequest request, CancellationToken ct = default);
    Task<Result<JournalEntryResponse>> PostAsync(Guid id, CancellationToken ct = default);
    Task<Result<JournalEntryResponse>> ReverseAsync(Guid id, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}