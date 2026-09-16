public interface ISupplierService
{
    Task<Result<List<SupplierResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<SupplierResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<SupplierResponse>> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default);
    Task<Result<SupplierResponse>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}