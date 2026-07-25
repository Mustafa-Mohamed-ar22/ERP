public record AccountResponse(Guid Id, string Code, string Name, string AccountType, Guid? ParentAccountId, bool IsActive);
