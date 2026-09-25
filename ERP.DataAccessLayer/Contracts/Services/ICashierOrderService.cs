public interface ICashierOrderService
{
    Task<Result<List<CashierOrderResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CashierOrderResponse>> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Result<CashierOrderResponse>> CreateOrderAsync(CreateCashierOrderRequest request, CancellationToken ct = default);
    Task<Result<CashierOrderResponse>> VoidOrderAsync(Guid orderId, VoidCashierOrderRequest request, CancellationToken ct = default);
    Task<Result<CashierInvoiceResponse>> GetInvoiceByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Result<CashierInvoiceResponse>> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default);

    Task<Result<List<CashierInvoiceResponse>>> GetMyInvoicesAsync(CancellationToken ct = default);
    Task<Result<List<CashierInvoiceResponse>>> GetInvoicesByUserIdAsync(Guid userId, CancellationToken ct = default);
}