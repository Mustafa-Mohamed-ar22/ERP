using Microsoft.EntityFrameworkCore;

public class AccountingIntegrationService : IAccountingIntegrationService
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryService _journalEntryService;
    private readonly ICurrentUserService _currentUser;

    public AccountingIntegrationService(ApplicationDbContext context, IJournalEntryService journalEntryService, ICurrentUserService currentUser)
    {
        _context = context;
        _journalEntryService = journalEntryService;
        _currentUser = currentUser;
    }

    public async Task<Result<GLPostingOutcome>> PostGoodsReceiptAsync(
        string orderNumber, List<(Guid ProductId, decimal Quantity, decimal UnitCost)> receivedLines, CancellationToken ct = default)
    {
        var settings = await _context.AccountingSettings.FirstOrDefaultAsync(s => s.CompanyId == _currentUser.CompanyId, ct);
        if (settings?.InventoryAccountId is null || settings.AccountsPayableAccountId is null)
            return Result.Success(GLPostingOutcome.Skipped);

        var totalCost = receivedLines.Sum(l => l.Quantity * l.UnitCost);
        if (totalCost <= 0)
            return Result.Success(GLPostingOutcome.Skipped);

        var lines = receivedLines
            .Where(l => l.Quantity * l.UnitCost > 0)
            .Select(l => new JournalEntryLineRequest(settings.InventoryAccountId!.Value, l.Quantity * l.UnitCost, 0, $"Receipt: {orderNumber}"))
            .ToList();
        lines.Add(new JournalEntryLineRequest(settings.AccountsPayableAccountId!.Value, 0, totalCost, $"AP: {orderNumber}"));

        return await CreateAndPostAsync($"Goods receipt - {orderNumber}", lines, ct);
    }

    public async Task<Result<GLPostingOutcome>> PostShipmentAsync(
        string orderNumber, List<(Guid ProductId, decimal Quantity, decimal SalePrice, decimal UnitCost)> shippedLines, CancellationToken ct = default)
    {
        var settings = await _context.AccountingSettings.FirstOrDefaultAsync(s => s.CompanyId == _currentUser.CompanyId, ct);
        if (settings?.AccountsReceivableAccountId is null || settings.RevenueAccountId is null
            || settings.InventoryAccountId is null || settings.CostOfGoodsSoldAccountId is null)
        {
            return Result.Success(GLPostingOutcome.Skipped);
        }

        var totalSale = shippedLines.Sum(l => l.Quantity * l.SalePrice);
        var totalCost = shippedLines.Sum(l => l.Quantity * l.UnitCost);
        if (totalSale <= 0 && totalCost <= 0)
            return Result.Success(GLPostingOutcome.Skipped);

        // Two paired entries in one JE: revenue recognition (AR/Revenue) + cost of goods sold (COGS/Inventory)
        var lines = new List<JournalEntryLineRequest>();
        if (totalSale > 0)
        {
            lines.Add(new JournalEntryLineRequest(settings.AccountsReceivableAccountId!.Value, totalSale, 0, $"Sale: {orderNumber}"));
            lines.Add(new JournalEntryLineRequest(settings.RevenueAccountId!.Value, 0, totalSale, $"Revenue: {orderNumber}"));
        }
        if (totalCost > 0)
        {
            lines.Add(new JournalEntryLineRequest(settings.CostOfGoodsSoldAccountId!.Value, totalCost, 0, $"COGS: {orderNumber}"));
            lines.Add(new JournalEntryLineRequest(settings.InventoryAccountId!.Value, 0, totalCost, $"Inventory relief: {orderNumber}"));
        }

        return await CreateAndPostAsync($"Shipment - {orderNumber}", lines, ct);
    }
    // AccountingIntegrationService.cs — add
    public async Task<Result<GLPostingResult>> PostCashSaleAsync(
        string orderNumber, List<(Guid ProductId, decimal Quantity, decimal SalePrice, decimal UnitCost)> lines, CancellationToken ct = default)
    {
        var settings = await _context.AccountingSettings.FirstOrDefaultAsync(s => s.CompanyId == _currentUser.CompanyId, ct);
        if (settings?.CashAccountId is null || settings.RevenueAccountId is null
            || settings.InventoryAccountId is null || settings.CostOfGoodsSoldAccountId is null)
            return Result.Success(new GLPostingResult(GLPostingOutcome.Skipped, null));

        var sale = lines.Sum(l => l.Quantity * l.SalePrice);
        var cost = lines.Sum(l => l.Quantity * l.UnitCost);
        if (sale <= 0) return Result.Success(new GLPostingResult(GLPostingOutcome.Skipped, null));

        var glLines = new List<JournalEntryLineRequest>
    {
        new(settings.CashAccountId!.Value, sale, 0, $"Cash sale: {orderNumber}"),
        new(settings.RevenueAccountId!.Value, 0, sale, $"Revenue: {orderNumber}")
    };
        if (cost > 0)
        {
            glLines.Add(new JournalEntryLineRequest(settings.CostOfGoodsSoldAccountId!.Value, cost, 0, $"COGS: {orderNumber}"));
            glLines.Add(new JournalEntryLineRequest(settings.InventoryAccountId!.Value, 0, cost, $"Inventory relief: {orderNumber}"));
        }

        var createResult = await _journalEntryService.CreateAsync(
            new CreateJournalEntryRequest(DateTime.UtcNow, $"Cashier sale - {orderNumber}", glLines), ct);
        if (!createResult.IsSuccess)
            return Result.Failure<GLPostingResult>(createResult.Error);

        var postResult = await _journalEntryService.PostAsync(createResult.Data!.Id, ct);
        if (!postResult.IsSuccess)
            return Result.Failure<GLPostingResult>(postResult.Error);

        return Result.Success(new GLPostingResult(GLPostingOutcome.Posted, createResult.Data!.Id));
    }
    private async Task<Result<GLPostingOutcome>> CreateAndPostAsync(string description, List<JournalEntryLineRequest> lines, CancellationToken ct)
    {
        var createResult = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest(DateTime.UtcNow, description, lines), ct);
        if (!createResult.IsSuccess)
            return Result.Failure<GLPostingOutcome>(createResult.Error);

        var postResult = await _journalEntryService.PostAsync(createResult.Data!.Id, ct);
        if (!postResult.IsSuccess)
            return Result.Failure<GLPostingOutcome>(postResult.Error);

        return Result.Success(GLPostingOutcome.Posted);
    }
}