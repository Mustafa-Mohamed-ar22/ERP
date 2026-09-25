using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Companies = new GenericRepository<Company>(_context);
        Branches = new GenericRepository<Branch>(_context);
        Departments = new GenericRepository<Department>(_context);
        Accounts = new GenericRepository<Account>(_context);
        JournalEntries = new GenericRepository<JournalEntry>(_context);
        Products = new GenericRepository<Product>(_context);
        Warehouses = new GenericRepository<Warehouse>(_context);
        StockItems = new GenericRepository<StockItem>(_context);
        StockMovements = new GenericRepository<StockMovement>(_context);
        ProductCategories = new GenericRepository<ProductCategory>(_context);
        Suppliers = new GenericRepository<Supplier>(_context);
        PurchaseOrders = new GenericRepository<PurchaseOrder>(_context);
        Customers = new GenericRepository<Customer>(_context);
        SalesOrders = new GenericRepository<SalesOrder>(_context);
        PurchaseOrderLines = new GenericRepository<PurchaseOrderLine>(_context);
        SalesOrderLines = new GenericRepository<SalesOrderLine>(_context);
        Employees = new GenericRepository<Employee>(_context);
        LeaveRequests = new GenericRepository<LeaveRequest>(_context);
        AttendanceRecords = new GenericRepository<AttendanceRecord>(_context);
        CashierShifts = new GenericRepository<CashierShift>(_context);
        CashierShiftCashMovements = new GenericRepository<CashierShiftCashMovement>(_context);
        CashierOrders = new GenericRepository<CashierOrder>(_context);
        CashierInvoices = new GenericRepository<CashierInvoice>(_context);
        AccountingSettings = new GenericRepository<AccountingSettings>(_context);
        Notifications = new GenericRepository<Notification>(_context);
    }

    public IGenericRepository<Company> Companies { get; }
    public IGenericRepository<Branch> Branches { get; }
    public IGenericRepository<Department> Departments { get; }

    public IGenericRepository<Account> Accounts { get; }

    public IGenericRepository<JournalEntry> JournalEntries { get; }
    public IGenericRepository<ProductCategory> ProductCategories { get; }

    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<Warehouse> Warehouses         { get; }
    public IGenericRepository<StockItem> StockItems         { get; }
    public IGenericRepository<StockMovement> StockMovements { get; }

    public IGenericRepository<Supplier> Suppliers           {get;}
    public IGenericRepository<PurchaseOrder> PurchaseOrders { get; }

    public IGenericRepository<Customer> Customers { get; }

    public IGenericRepository<SalesOrder> SalesOrders { get; }

    public IGenericRepository<PurchaseOrderLine> PurchaseOrderLines { get; }

    public IGenericRepository<SalesOrderLine> SalesOrderLines { get; }

    public IGenericRepository<Employee> Employees{ get; }

    public IGenericRepository<LeaveRequest> LeaveRequests { get; }

    public IGenericRepository<AttendanceRecord> AttendanceRecords { get; }

    public IGenericRepository<CashierShift> CashierShifts { get; }

    public IGenericRepository<CashierShiftCashMovement> CashierShiftCashMovements { get; }

    public IGenericRepository<CashierOrder> CashierOrders { get; }

    public IGenericRepository<CashierInvoice> CashierInvoices { get; }

    public IGenericRepository<AccountingSettings> AccountingSettings { get; }

    public IGenericRepository<Notification> Notifications { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => _context.Database.BeginTransactionAsync(ct);


    public IExecutionStrategy CreateExecutionStrategy()
        => _context.Database.CreateExecutionStrategy();
}