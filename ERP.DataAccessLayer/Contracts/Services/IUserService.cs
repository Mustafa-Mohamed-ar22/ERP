public interface IUserService
{
    Task<Result<List<UserResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task<Result> AssignRolesAsync(Guid id, AssignUserRolesRequest request, CancellationToken ct = default);
    Task<Result> DeactivateAsync(Guid id, CancellationToken ct = default);
}