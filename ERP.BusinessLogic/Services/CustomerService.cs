using Microsoft.EntityFrameworkCore;
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CustomerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CustomerResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var customers = await _unitOfWork.Customers.Query().ToListAsync(ct);
        return Result.Success(customers.Select(ToResponse).ToList());
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, ct);
        if (customer is null)
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);

        return Result.Success(ToResponse(customer));
    }

    public async Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        
        if (request.Email is not null)
        {
            var existingCustomerEmail = await _unitOfWork.Customers.Query()
            .AnyAsync(c => c.Email == request.Email, ct);
            if (existingCustomerEmail)
                return Result.Failure<CustomerResponse>(CustomerErrors.EmailAlreadyExists);
        }
        if (request.Phone is not null)
        {
            var existingCustomerPhone = await _unitOfWork.Customers.Query()
           .AnyAsync(c => c.Phone == request.Phone, ct);
            if (existingCustomerPhone)
                return Result.Failure<CustomerResponse>(CustomerErrors.PhoneAlreadyExists);
        }
       
        var customer = new Customer
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

        await _unitOfWork.Customers.AddAsync(customer, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(customer));
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, ct);
        if (customer is null)
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        if (request.Email is not null)
        {
            var existingCustomerEmail = await _unitOfWork.Customers.Query()
               .AnyAsync(c => c.Id != id && c.Email == request.Email, ct);
            if (existingCustomerEmail)
                return Result.Failure<CustomerResponse>(CustomerErrors.EmailAlreadyExists);
        }

        if (request.Phone is not null)
        {
            var existingCustomerPhone = await _unitOfWork.Customers.Query()
                   .AnyAsync(c => c.Id != id && c.Phone == request.Phone, ct);
            if (existingCustomerPhone)
                return Result.Failure<CustomerResponse>(CustomerErrors.PhoneAlreadyExists);
        }
      
        customer.Name = request.Name;
        customer.ContactName = request.ContactName;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Address = request.Address;
        customer.TaxNumber = request.TaxNumber;
        customer.IsActive = request.IsActive;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(customer));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, ct);
        if (customer is null)
            return Result.Failure(CustomerErrors.NotFound);

        var hasOrders = await _unitOfWork.SalesOrders.Query().AnyAsync(o => o.CustomerId == id, ct);
        if (hasOrders)
            return Result.Failure(CustomerErrors.HasOrders);

        customer.IsDeleted = true;
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
    private static CustomerResponse ToResponse(Customer c) => new(
        c.Id, c.Name, c.ContactName, c.Phone, c.Email, c.Address, c.TaxNumber, c.IsActive);
}
