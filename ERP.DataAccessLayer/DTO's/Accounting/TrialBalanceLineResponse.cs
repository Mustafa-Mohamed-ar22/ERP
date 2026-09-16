public record TrialBalanceLineResponse(
    Guid AccountId, string Code, string Name, string AccountType,
    decimal TotalDebit, decimal TotalCredit,      // raw movement turnover
    decimal DebitBalance, decimal CreditBalance); // classic trial balance columns — exactly one is nonzero per line
