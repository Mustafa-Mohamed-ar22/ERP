public interface IEmployeeService
{
    Task<Result<List<EmployeeResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<EmployeeResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<EmployeeResponse>> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
    Task<Result<EmployeeResponse>> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserResponse>> GrantAccessAsync(Guid employeeId, GrantEmployeeAccessRequest request, CancellationToken ct = default);
}
