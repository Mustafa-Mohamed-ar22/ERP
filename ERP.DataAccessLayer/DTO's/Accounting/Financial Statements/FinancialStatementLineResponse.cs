
public record FinancialStatementLineResponse(Guid AccountId, string Code, string Name, decimal Balance);

public record BalanceSheetResponse(
    DateTime AsOfDate,
    List<FinancialStatementLineResponse> Assets, decimal TotalAssets,
    List<FinancialStatementLineResponse> Liabilities, decimal TotalLiabilities,
    List<FinancialStatementLineResponse> Equity, decimal CurrentEarnings, decimal TotalEquity,
    bool IsBalanced);

public record IncomeStatementResponse(
    DateTime PeriodStart, DateTime PeriodEnd,
    List<FinancialStatementLineResponse> Revenues, decimal TotalRevenue,
    List<FinancialStatementLineResponse> Expenses, decimal TotalExpenses,
    decimal NetIncome);

public record CashFlowLineResponse(DateTime Date, string Description, decimal Debit, decimal Credit, decimal RunningBalance);
public record CashFlowSummaryResponse(
    DateTime PeriodStart, DateTime PeriodEnd,
    decimal OpeningCashBalance, decimal ClosingCashBalance, decimal NetChange,
    List<CashFlowLineResponse> Movements);