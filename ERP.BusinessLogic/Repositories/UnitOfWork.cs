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
    }

    public IGenericRepository<Company> Companies { get; }
    public IGenericRepository<Branch> Branches { get; }
    public IGenericRepository<Department> Departments { get; }

    public IGenericRepository<Account> Accounts { get; }

    public IGenericRepository<JournalEntry> JournalEntries { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => _context.Database.BeginTransactionAsync(ct);
}