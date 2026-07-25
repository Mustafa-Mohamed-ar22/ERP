public record CreateJournalEntryRequest(DateTime EntryDate, string? Description, List<JournalEntryLineRequest> Lines);
