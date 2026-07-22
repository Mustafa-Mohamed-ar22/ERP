using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork
{
    IGenericRepository<Company> Companies { get; }
    IGenericRepository<Branch> Branches { get; }
    IGenericRepository<Department> Departments { get; }
    // Add one property per aggregate root as modules come online, e.g.:
    // IGenericRepository<Invoice> Invoices { get; }
    // Repository<T>(); // more generic way to get a repository for any entity type, if needed
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}