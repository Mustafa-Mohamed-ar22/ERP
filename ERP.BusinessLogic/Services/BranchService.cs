using Mapster;
using Microsoft.EntityFrameworkCore;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public BranchService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    // Note: no manual CompanyId filtering needed below — Branch implements ITenantEntity,
    // so the global query filter already scopes every one of these to the caller's own company.

    public async Task<Result<List<BranchResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var branches = await _unitOfWork.Branches.Query().ToListAsync(ct);
        return Result.Success(branches.Adapt<List<BranchResponse>>());
    }

    public async Task<Result<BranchResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id, ct);
        if (branch is null)
            return Result.Failure<BranchResponse>(BranchErrors.NotFound);

        return Result.Success(branch.Adapt<BranchResponse>());
    }
    public async Task<Result<List<DepartmentResponse>>> GetDepartmentsByBranchIdAsync(Guid id, CancellationToken ct = default)
    {
        var departments = await _unitOfWork.Departments.Query().Where(x => x.BranchId == id).ToListAsync(ct);
        if (departments is null)
            return Result.Failure<List<DepartmentResponse>>(DepartmentErrors.NotFound);

        return Result.Success(departments.Adapt<List<DepartmentResponse>>());
    }
    public async Task<Result<BranchResponse>> CreateAsync(CreateBranchRequest request, CancellationToken ct = default)
    {
        var duplicateExists = await _unitOfWork.Branches.Query().AnyAsync(b => b.Code == request.Code, ct);
        if (duplicateExists)
            return Result.Failure<BranchResponse>(BranchErrors.DuplicateCode);

        var branch = new Branch
        {
            CompanyId = _currentUser.CompanyId,
            Name = request.Name,
            Code = request.Code,
            Address = request.Address,
            Phone = request.Phone,
            IsMain = request.IsMain,
            IsActive = true
        };

        await _unitOfWork.Branches.AddAsync(branch, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(branch.Adapt<BranchResponse>());
    }

    public async Task<Result<BranchResponse>> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id, ct);
        if (branch is null)
            return Result.Failure<BranchResponse>(BranchErrors.NotFound);

        var duplicateExists = await _unitOfWork.Branches.Query()
            .AnyAsync(b => b.Code == request.Code && b.Id != id, ct);
        if (duplicateExists)
            return Result.Failure<BranchResponse>(BranchErrors.DuplicateCode);

        branch.Name = request.Name;
        branch.Code = request.Code;
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.IsMain = request.IsMain;
        branch.IsActive = request.IsActive;

        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(branch.Adapt<BranchResponse>());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id, ct);
        if (branch is null)
            return Result.Failure(BranchErrors.NotFound);

        branch.IsDeleted = true;   // soft delete — global query filter f7777oooolllly
        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}