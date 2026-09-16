using Microsoft.EntityFrameworkCore;
public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SupplierService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<SupplierResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var suppliers = await _unitOfWork.Suppliers.Query().ToListAsync(ct);
        return Result.Success(suppliers.Select(ToResponse).ToList());
    }
    public async Task<Result<SupplierResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, ct);
        if (supplier is null)
            return Result.Failure<SupplierResponse>(SupplierErrors.NotFound);

        return Result.Success(ToResponse(supplier));
    }

    public async Task<Result<SupplierResponse>> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default)
    {
        if(request.Email is not null)
        {
            var existingSupplierEmail = await _unitOfWork.Suppliers.Query()
                  .AnyAsync(c => c.Email == request.Email, ct);
            if (existingSupplierEmail)
                return Result.Failure<SupplierResponse>(SupplierErrors.EmailAlreadyExists);
        }
        if(request.Phone is not null)
        {
            var existingSupplierPhone = await _unitOfWork.Suppliers.Query()
          .AnyAsync(c => c.Phone == request.Phone, ct);
            if (existingSupplierPhone)
                return Result.Failure<SupplierResponse>(SupplierErrors.PhoneAlreadyExists);
        }

      
        var supplier = new Supplier
        {
            CompanyId = _currentUser.CompanyId,
            Name = request.Name,
            ContactName = request.ContactName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            IsActive = true
        };

        await _unitOfWork.Suppliers.AddAsync(supplier, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(supplier));
    }

    public async Task<Result<SupplierResponse>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, ct);
        if (supplier is null)
            return Result.Failure<SupplierResponse>(SupplierErrors.NotFound);
        if (request.Email is not null)
        {
            var existingSupplierEmail = await _unitOfWork.Suppliers.Query()
           .AnyAsync(c => c.Id != id && c.Email == request.Email, ct);
            if (existingSupplierEmail)
                return Result.Failure<SupplierResponse>(SupplierErrors.EmailAlreadyExists);
        }

        if (request.Phone is not null)
        {
            var existingSupplierPhone = await _unitOfWork.Suppliers.Query()
           .AnyAsync(c => c.Id != id && c.Phone == request.Phone, ct);
            if (existingSupplierPhone)
                return Result.Failure<SupplierResponse>(SupplierErrors.PhoneAlreadyExists);
        }
       
        supplier.Name = request.Name;
        supplier.ContactName = request.ContactName;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.Address = request.Address;
        supplier.TaxNumber = request.TaxNumber;
        supplier.IsActive = request.IsActive;

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(supplier));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, ct);
        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound);

        var hasOrders = await _unitOfWork.PurchaseOrders.Query().AnyAsync(o => o.SupplierId == id, ct);
        if (hasOrders)
            return Result.Failure(SupplierErrors.HasOrders);

        supplier.IsDeleted = true;
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
    private static SupplierResponse ToResponse(Supplier s) => new(
        s.Id, s.Name, s.ContactName, s.Phone, s.Email, s.Address, s.TaxNumber, s.IsActive);
}
