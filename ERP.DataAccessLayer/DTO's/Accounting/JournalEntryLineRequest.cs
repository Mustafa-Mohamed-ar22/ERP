// DTO's/Accounting/JournalEntryLineRequest.cs
public record JournalEntryLineRequest(Guid AccountId, decimal Debit, decimal Credit, string? Description);
