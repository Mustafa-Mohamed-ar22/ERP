// DTO's/Accounting/JournalEntryLineResponse.cs
public record JournalEntryLineResponse(Guid Id, Guid AccountId, string AccountCode, string AccountName, decimal Debit, decimal Credit, string? Description);
