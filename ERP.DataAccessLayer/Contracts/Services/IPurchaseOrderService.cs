public interface IPurchaseOrderService
{
    Task<Result<List<PurchaseOrderResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<PurchaseOrderResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<PurchaseOrderResponse>> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken ct = default);
    Task<Result<PurchaseOrderResponse>> SubmitAsync(Guid id, CancellationToken ct = default);
    Task<Result<PurchaseOrderResponse>> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<Result<PurchaseOrderResponse>> ReceiveGoodsAsync(Guid id, ReceiveGoodsRequest request, CancellationToken ct = default);
    Task<Result<PurchaseOrderResponse>> CancelAsync(Guid id, CancellationToken ct = default);
    Task<Result<PurchaseOrderResponse>> UpdateAsync(Guid id, CreatePurchaseOrderRequest request, CancellationToken ct = default);
}