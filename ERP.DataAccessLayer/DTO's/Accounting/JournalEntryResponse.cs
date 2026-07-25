// DTO's/Accounting/JournalEntryResponse.cs
public record JournalEntryResponse(
    Guid Id, string EntryNumber, DateTime EntryDate, string? Description, string Status,
    Guid? ReversalOfEntryId, List<JournalEntryLineResponse> Lines);