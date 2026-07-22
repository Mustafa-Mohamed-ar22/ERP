using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }
    public Guid CurrentCompanyId => _tenantProvider.CompanyId;
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var filter = BuildTenantAndSoftDeleteFilter(entityType.ClrType);
            if (filter is not null)
                entityType.SetQueryFilter(filter);
        }
    }

    private LambdaExpression? BuildTenantAndSoftDeleteFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");
        Expression? filter = null;

        if (typeof(ITenantEntity).IsAssignableFrom(clrType))
        {
            var companyIdProperty = Expression.Property(parameter, nameof(ITenantEntity.CompanyId));

            // Constant(this) is specially re-bound by EF Core to the *actual executing* context instance —
            // this is what makes the filter correct per-request despite the model being cached once.
            var contextInstance = Expression.Constant(this);
            var currentCompanyId = Expression.Property(contextInstance, nameof(CurrentCompanyId));

            filter = Expression.Equal(companyIdProperty, currentCompanyId);
        }

        if (typeof(ISoftDelete).IsAssignableFrom(clrType))
        {
            var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var notDeleted = Expression.Not(isDeletedProperty);
            filter = filter is null ? notDeleted : Expression.AndAlso(filter, notDeleted);
        }

        return filter is null ? null : Expression.Lambda(filter, parameter);
    }
}