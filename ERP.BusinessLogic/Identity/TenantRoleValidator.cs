using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.BusinessLogic.Identity
{
    public class TenantRoleValidator : RoleValidator<ApplicationRole>
    {
        public override async Task<IdentityResult> ValidateAsync(RoleManager<ApplicationRole> manager, ApplicationRole role)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(role.Name))
            {
                errors.Add(new IdentityError { Code = "InvalidRoleName", Description = "Role name cannot be empty." });
            }
            else
            {
                var normalizedName = manager.NormalizeKey(role.Name);

                var duplicateExists = await manager.Roles.IgnoreQueryFilters()
                    .AnyAsync(r => r.CompanyId == role.CompanyId && r.NormalizedName == normalizedName && r.Id != role.Id);

                if (duplicateExists)
                {
                    errors.Add(new IdentityError
                    {
                        Code = "DuplicateRoleName",
                        Description = $"Role '{role.Name}' already exists for this company."
                    });
                }
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }
    }
}