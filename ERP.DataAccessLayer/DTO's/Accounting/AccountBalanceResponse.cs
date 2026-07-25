// DTO's/Accounting/AccountBalanceResponse.cs
public record AccountBalanceResponse(Guid AccountId, string Code, string Name, decimal TotalDebit, decimal TotalCredit, decimal Balance);
