using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    public AccountsController(IAccountService accountService) => _accountService = accountService;

    [HttpGet]
    [Authorize(Policy = "accounting.accounts.view")]
    public async Task<ActionResult<List<AccountResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:Guid}")]
    [Authorize(Policy = "accounting.accounts.view")]
    public async Task<ActionResult<AccountResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}/balance")]
    [Authorize(Policy = "accounting.accounts.view")]
    public async Task<ActionResult<AccountBalanceResponse>> GetBalanceAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.GetBalanceAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "accounting.accounts.manage")]
    public async Task<ActionResult<AccountResponse>> CreateAsync(
        [FromBody] CreateAccountRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut()]
    [Authorize(Policy = "accounting.accounts.manage")]
    public async Task<ActionResult<AccountResponse>> UpdateAsync(
        [FromQuery]Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "accounting.accounts.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpGet("account-types")]
    [Authorize(Policy = "accounting.accounts.view")]
    public ActionResult<List<AccountTypeResponse>> GetAccountTypes()
    {
        var types = Enum.GetValues<AccountType>()
            .Select(t => new AccountTypeResponse(
                t.ToString(),
                t is AccountType.Asset or AccountType.Expense ? "Debit" : "Credit"))
            .ToList();

        return Ok(types);
    }
    [HttpGet("trial-balance")]
    [Authorize(Policy = "accounting.accounts.view")]
    public async Task<ActionResult<TrialBalanceResponse>> GetTrialBalanceAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.GetTrialBalanceAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
    [HttpGet("balance-sheet")]
    [Authorize(Policy = "accounting.accounts.view")]
    public async Task<ActionResult<BalanceSheetResponse>> GetBalanceSheetAsync(
    [FromQuery] DateTime? asOfDate, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.GetBalanceSheetAsync(asOfDate ?? DateTime.UtcNow, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("income-statement")]
    [Authorize(Policy = "accounting.accounts.view")]
    public async Task<ActionResult<IncomeStatementResponse>> GetIncomeStatementAsync(
        [FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.GetIncomeStatementAsync(periodStart, periodEnd, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("cash-flow-summary")]
    [Authorize(Policy = "accounting.accounts.view")]
    public async Task<ActionResult<CashFlowSummaryResponse>> GetCashFlowSummaryAsync(
        [FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, CancellationToken cancellationToken = default!)
    {
        var result = await _accountService.GetCashFlowSummaryAsync(periodStart, periodEnd, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }
}