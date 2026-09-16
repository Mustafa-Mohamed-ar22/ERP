public interface IWarehouseService
{
    Task<Result<List<WarehouseResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<WarehouseResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<WarehouseResponse>> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default);
    Task<Result<WarehouseResponse>> UpdateAsync(Guid id, UpdateWarehouseRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}