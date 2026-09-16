public interface IProductService
{
    Task<Result<List<ProductResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ProductResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<Result<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}