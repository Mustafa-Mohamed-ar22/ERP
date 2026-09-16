using Microsoft.EntityFrameworkCore;

public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public WarehouseService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<WarehouseResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var warehouses = await _unitOfWork.Warehouses.Query().ToListAsync(ct);
        return Result.Success(warehouses.Select(ToResponse).ToList());
    }

    public async Task<Result<WarehouseResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, ct);
        if (warehouse is null)
            return Result.Failure<WarehouseResponse>(WarehouseErrors.NotFound);

        return Result.Success(ToResponse(warehouse));
    }
    public async Task<Result<WarehouseResponse>> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        var duplicateExists = await _unitOfWork.Warehouses.Query().AnyAsync(w => w.Code == request.Code, ct);
        if (duplicateExists)
            return Result.Failure<WarehouseResponse>(WarehouseErrors.DuplicateCode);
        var isBranchExisted = await _unitOfWork.Branches.Query().AnyAsync(x => x.Id == request.BranchId);
        if (!isBranchExisted)
            return Result.Failure<WarehouseResponse>(BranchErrors.NotFound);
        var existingWarehouseName = await _unitOfWork.Warehouses.Query()
            .FirstOrDefaultAsync(w => w.Name == request.Name, ct);
        if (existingWarehouseName != null)
            return Result.Failure<WarehouseResponse>(WarehouseErrors.DuplicateName);
        var warehouse = new Warehouse
        {
            CompanyId = _currentUser.CompanyId,
            Name = request.Name,
            Code = request.Code,
            BranchId = request.BranchId,
            IsActive = true
        };
        await _unitOfWork.Warehouses.AddAsync(warehouse, ct);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<WarehouseResponse>(WarehouseErrors.DuplicateCode);
        }
        return Result.Success(ToResponse(warehouse));
    }

    public async Task<Result<WarehouseResponse>> UpdateAsync(Guid id, UpdateWarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, ct);
        if (warehouse is null)
            return Result.Failure<WarehouseResponse>(WarehouseErrors.NotFound);

        var duplicateExists = await _unitOfWork.Warehouses.Query()
            .AnyAsync(w => w.Code == request.Code && w.Id != id, ct);
        if (duplicateExists)
            return Result.Failure<WarehouseResponse>(WarehouseErrors.DuplicateCode);
        var isBranchExisted = await _unitOfWork.Branches.Query().AnyAsync(x => x.Id == request.BranchId);
        if (!isBranchExisted)
            return Result.Failure<WarehouseResponse>(BranchErrors.NotFound);
        var existingWarehouseName = await _unitOfWork.Warehouses.Query()
           .FirstOrDefaultAsync(w => w.Id!=id&& w.Name == request.Name, ct);
        if (existingWarehouseName != null)
            return Result.Failure<WarehouseResponse>(WarehouseErrors.DuplicateName);
        warehouse.Name = request.Name;
        warehouse.Code = request.Code;
        warehouse.BranchId = request.BranchId;
        warehouse.IsActive = request.IsActive;

        _unitOfWork.Warehouses.Update(warehouse);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<WarehouseResponse>(WarehouseErrors.DuplicateCode);
        }
        return Result.Success(ToResponse(warehouse));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, ct);
        if (warehouse is null)
            return Result.Failure(WarehouseErrors.NotFound);

        var hasStock = await _unitOfWork.StockItems.Query().AnyAsync(s => s.WarehouseId == id && s.QuantityOnHand != 0, ct);
        if (hasStock)
            return Result.Failure(WarehouseErrors.HasStock);

        warehouse.IsDeleted = true;
        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static WarehouseResponse ToResponse(Warehouse w) => new(w.Id, w.Name, w.Code, w.BranchId, w.IsActive);
}
