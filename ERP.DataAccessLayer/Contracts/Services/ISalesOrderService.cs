public interface ISalesOrderService
{
    Task<Result<List<SalesOrderResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<SalesOrderResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<SalesOrderResponse>> CreateAsync(CreateSalesOrderRequest request, CancellationToken ct = default);
    Task<Result<SalesOrderResponse>> SubmitAsync(Guid id, CancellationToken ct = default);
    Task<Result<SalesOrderResponse>> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<Result<SalesOrderResponse>> ShipGoodsAsync(Guid id, ShipGoodsRequest request, CancellationToken ct = default);
    Task<Result<SalesOrderResponse>> CancelAsync(Guid id, CancellationToken ct = default);
    Task<Result<SalesOrderResponse>> UpdateAsync(Guid id, CreateSalesOrderRequest request, CancellationToken ct = default);
    
}