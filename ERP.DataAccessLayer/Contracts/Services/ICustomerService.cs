public interface ICustomerService
{
    Task<Result<List<CustomerResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CustomerResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}