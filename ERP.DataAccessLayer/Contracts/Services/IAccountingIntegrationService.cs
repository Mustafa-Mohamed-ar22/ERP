public interface IAccountingIntegrationService
{
    Task<Result<GLPostingOutcome>> PostGoodsReceiptAsync(
        string orderNumber, List<(Guid ProductId, decimal Quantity, decimal UnitCost)> receivedLines, CancellationToken ct = default);

    Task<Result<GLPostingOutcome>> PostShipmentAsync(
        string orderNumber, List<(Guid ProductId, decimal Quantity, decimal SalePrice, decimal UnitCost)> shippedLines, CancellationToken ct = default);
}