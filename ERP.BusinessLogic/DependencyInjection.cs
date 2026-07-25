using ERP.BusinessLogic.Identity;
using ERP.BusinessLogic.Services;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
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
            options.SignIn.RequireConfirmedEmail = true;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Replace Identity's global role-name uniqueness check with a tenant-scoped one
        services.RemoveAll<IRoleValidator<ApplicationRole>>();
        services.AddScoped<IRoleValidator<ApplicationRole>, TenantRoleValidator>();
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IJournalEntryService, JournalEntryService>();


        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.Configure<DomainCORS>(config.GetSection("DomainCORS"));                   /// CORS POLICY

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://synaptech-erp.vercel.app", "https://localhost:7086/")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IBranchService, BranchService>();



        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IRoleService, RoleService>();

        //services.AddScoped<ICompanyRepository, CompanyRepository>();
        //services.AddScoped<IBranchRepository, BranchRepository>();

        //services.AddScoped<IRoleService, RoleService>();
        //services.AddScoped<IAuditService, AuditService>();
        //services.AddScoped<INotificationService, NotificationService>();

        services.AddMapster();
        services.AddValidators();
        return services;
    }
    private static IServiceCollection AddValidators(
          this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();
        return services;
    }
}