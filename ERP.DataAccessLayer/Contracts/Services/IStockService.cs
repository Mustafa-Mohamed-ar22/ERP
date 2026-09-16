public interface IStockService
{
    Task<Result<List<StockLevelResponse>>> GetProductStockAsync(Guid productId, CancellationToken ct = default);
    Task<Result<List<StockLevelResponse>>> GetWarehouseStockAsync(Guid warehouseId, CancellationToken ct = default);
    Task<Result<StockMovementResponse>> RecordMovementAsync(RecordStockMovementRequest request, CancellationToken ct = default);
    Task<Result<List<StockMovementResponse>>> TransferAsync(TransferStockRequest request, CancellationToken ct = default);
}