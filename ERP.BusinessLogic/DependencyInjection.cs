using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();
        //services.AddScoped<ICurrentUserService, CurrentUserService>();

        //services.AddScoped<IUnitOfWork, UnitOfWork>();
        //services.AddScoped<ICompanyRepository, CompanyRepository>();
        //services.AddScoped<IBranchRepository, BranchRepository>();

        //services.AddScoped<IAuthService, AuthService>();
        //services.AddScoped<ICompanyService, CompanyService>();
        //services.AddScoped<IBranchService, BranchService>();
        //services.AddScoped<IUserService, UserService>();
        //services.AddScoped<IRoleService, RoleService>();
        //services.AddScoped<IAuditService, AuditService>();
        //services.AddScoped<INotificationService, NotificationService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMapster();
        // // // // // // // services.AddFluentValidationAutoValidation();

        return services;
    }
    //private static IServiceCollection AddValidators(
    //        this IServiceCollection services)
    //{
    //    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    //    return services;
    //}
}