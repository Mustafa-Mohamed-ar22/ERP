public interface IDepartmentService
{
    Task<Result<List<DepartmentResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<DepartmentResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request, CancellationToken ct = default);
    Task<Result<DepartmentResponse>> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
} 