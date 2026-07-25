using Mapster;
using Microsoft.EntityFrameworkCore;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DepartmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    // No manual CompanyId filtering needed — Department implements ITenantEntity,
    // the global query filter already scopes everything to the caller's company.

    public async Task<Result<List<DepartmentResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var departments = await _unitOfWork.Departments.Query().ToListAsync(ct);
        return Result.Success(departments.Adapt<List<DepartmentResponse>>());
    }

    public async Task<Result<DepartmentResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id, ct);
        if (department is null)
            return Result.Failure<DepartmentResponse>(DepartmentErrors.NotFound);

        return Result.Success(department.Adapt<DepartmentResponse>());
    }
    
    public async Task<Result<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request, CancellationToken ct = default)
    {
        if (request.ParentDepartmentId is { } parentId)
        {
            var parentExists = await _unitOfWork.Departments.Query().AnyAsync(d => d.Id == parentId, ct);
            if (!parentExists)
                return Result.Failure<DepartmentResponse>(DepartmentErrors.ParentNotFound);
        }

        var department = new Department
        {
            CompanyId = _currentUser.CompanyId,
            Name = request.Name,
            BranchId = request.BranchId,
            ParentDepartmentId = request.ParentDepartmentId,
            IsActive = true
        };

        await _unitOfWork.Departments.AddAsync(department, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(department.Adapt<DepartmentResponse>());
    }

    public async Task<Result<DepartmentResponse>> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken ct = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id, ct);
        if (department is null)
            return Result.Failure<DepartmentResponse>(DepartmentErrors.NotFound);

        if (request.ParentDepartmentId is { } parentId)
        {
            if (parentId == id)
                return Result.Failure<DepartmentResponse>(DepartmentErrors.InvalidParent);

            var parentExists = await _unitOfWork.Departments.Query().AnyAsync(d => d.Id == parentId, ct);
            if (!parentExists)
                return Result.Failure<DepartmentResponse>(DepartmentErrors.ParentNotFound);

            // Walk up the new parent's ancestor chain — reject if `id` appears anywhere in it (will create a cycle f7olya)
            var currentAncestorId = (Guid?)parentId;
            var depth = 0;
            while (currentAncestorId is not null && depth < 50)
            {
                if (currentAncestorId == id)
                    return Result.Failure<DepartmentResponse>(DepartmentErrors.InvalidParent);

                currentAncestorId = await _unitOfWork.Departments.Query()
                    .Where(d => d.Id == currentAncestorId)
                    .Select(d => d.ParentDepartmentId)
                    .FirstOrDefaultAsync(ct);
                depth++;
            }
        }

        department.Name = request.Name;
        department.BranchId = request.BranchId;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.IsActive = request.IsActive;

        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(department.Adapt<DepartmentResponse>());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id, ct);
        if (department is null)
            return Result.Failure(DepartmentErrors.NotFound);

        var hasChildren = await _unitOfWork.Departments.Query().AnyAsync(d => d.ParentDepartmentId == id, ct);
        if (hasChildren)
            return Result.Failure(DepartmentErrors.InvalidParent); // reuse: "can't delete, still has sub-departments"

        department.IsDeleted = true;
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}