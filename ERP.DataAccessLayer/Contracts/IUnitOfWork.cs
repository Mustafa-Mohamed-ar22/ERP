using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork
{
    IGenericRepository<Company> Companies { get; }
    IGenericRepository<Branch> Branches { get; }
    IGenericRepository<Department> Departments { get; }
    // Add one property per aggregate root as modules come online, e.g.:
    // IGenericRepository<Invoice> Invoices { get; }
    // Repository<T>(); // more generic way to get a repository for any entity type, if needed



    IGenericRepository<Account> Accounts { get; }             
    IGenericRepository<JournalEntry> JournalEntries { get; }

    IGenericRepository<Product> Products { get; }
    IGenericRepository<Warehouse> Warehouses { get; }
    IGenericRepository<StockItem> StockItems { get; }
    IGenericRepository<StockMovement> StockMovements { get; }
    IGenericRepository<ProductCategory> ProductCategories { get; }
    IGenericRepository<PurchaseOrderLine> PurchaseOrderLines { get; }
    IGenericRepository<SalesOrderLine> SalesOrderLines { get; }
    IGenericRepository<Supplier> Suppliers { get; }
    IGenericRepository<PurchaseOrder> PurchaseOrders { get; }

    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<SalesOrder> SalesOrders { get; }


    IGenericRepository<Employee> Employees { get; }
    IGenericRepository<LeaveRequest> LeaveRequests { get; }
    IGenericRepository<AttendanceRecord> AttendanceRecords { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    IExecutionStrategy CreateExecutionStrategy();

}