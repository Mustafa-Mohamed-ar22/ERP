using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.MigrateAsync();

        // 1. Seed permissions catalog (idempotent)
        foreach (var (code, module, description) in PermissionsCatalog.All)
        {
            if (!await context.Permissions.IgnoreQueryFilters().AnyAsync(p => p.Code == code))
            {
                context.Permissions.Add(new Permission
                {
                    Code = code,
                    Module = module,
                    Description = description
                });
            }
        }
        await context.SaveChangesAsync();

        // 2. Seed a default demo company (only if none exists)
        var defaultCompany = await context.Companies.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (defaultCompany is null)
        {
            defaultCompany = new Company
            {
                Name = "Demo Company",
                Currency = "EGP",
                Country = "Egypt",
                IsActive = true
            };
            context.Companies.Add(defaultCompany);

            var mainBranch = new Branch
            {
                CompanyId = defaultCompany.Id,
                Name = "Head Office",
                Code = "HQ",
                IsMain = true
            };
            context.Branches.Add(mainBranch);

            await context.SaveChangesAsync();
        }

        // 3. Seed system role: SuperAdmin (CompanyId = null => global)
        var superAdminExists = await context.Roles
                                    .IgnoreQueryFilters()
                                    .AnyAsync(r => r.Name == "SuperAdmin");
        if (!superAdminExists)
        {
            var superAdminRole = new ApplicationRole
            {
                Name = "SuperAdmin",
                CompanyId = null,
                Description = "Full system access across all modules"
            };
            await roleManager.CreateAsync(superAdminRole);

            var allPermissionIds = await context.Permissions.IgnoreQueryFilters()
                .Select(p => p.Id).ToListAsync();

            foreach (var permissionId in allPermissionIds)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = permissionId
                });
            }
            await context.SaveChangesAsync();
        }

        // 4. Seed default admin user for the demo company
        var adminEmail = "admin@synaptech.local";

        var adminExists = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == adminEmail);

        if (!adminExists)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                CompanyId = defaultCompany.Id,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "ChangeMe@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }
    }
}