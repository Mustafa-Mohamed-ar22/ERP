
public interface IBranchService
{
    Task<Result<List<BranchResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<BranchResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<BranchResponse>> CreateAsync(CreateBranchRequest request, CancellationToken ct = default);
    Task<Result<BranchResponse>> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<Result<List<DepartmentResponse>>> GetDepartmentsByBranchIdAsync(Guid id, CancellationToken ct = default!);

}