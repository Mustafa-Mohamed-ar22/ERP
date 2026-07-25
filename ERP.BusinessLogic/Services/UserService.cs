using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _emailService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly DomainCORS _domainOptions;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ICurrentUserService currentUser,
        IEmailSender emailService,
        IWebHostEnvironment webHostEnvironment,
        IOptions<DomainCORS> options,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUser = currentUser;
        _emailService = emailService;
        _webHostEnvironment = webHostEnvironment;
        _domainOptions = options.Value;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    private async Task<Error?> ValidateBranchAndDepartmentAsync(Guid? branchId, Guid? departmentId, CancellationToken ct)
    {
        if (branchId is { } bId)
        {
            var branchExists = await _unitOfWork.Branches.Query().AnyAsync(b => b.Id == bId, ct);
            if (!branchExists)
                return UserErrors.BranchNotFound;
        }

        if (departmentId is { } dId)
        {
            var department = await _unitOfWork.Departments.Query().FirstOrDefaultAsync(d => d.Id == dId, ct);
            if (department is null)
                return UserErrors.DepartmentNotFound;

            // Only enforce the match if the department IS tied to a specific branch —
            // a company-wide department (BranchId == null) is valid alongside any branch.
            if (branchId is { } b2 && department.BranchId is not null && department.BranchId != b2)
                return UserErrors.DepartmentBranchMismatch;
        }

        return null;
    }
    public async Task<Result<List<UserResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _userManager.Users
            .Where(u => u.CompanyId == _currentUser.CompanyId)
            .ToListAsync(ct);

        var branchIds = users.Where(u => u.BranchId.HasValue).Select(u => u.BranchId!.Value).Distinct().ToList();
        var departmentIds = users.Where(u => u.DepartmentId.HasValue).Select(u => u.DepartmentId!.Value).Distinct().ToList();

        var branchNames = await _unitOfWork.Branches.Query()
            .Where(b => branchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);

        var departmentNames = await _unitOfWork.Departments.Query()
            .Where(d => departmentIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Name, ct);

        var responses = new List<UserResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var branchName = user.BranchId is { } bId && branchNames.TryGetValue(bId, out var bn) ? bn : null;
            var departmentName = user.DepartmentId is { } dId && departmentNames.TryGetValue(dId, out var dn) ? dn : null;
            responses.Add(ToResponse(user, roles, branchName, departmentName));
        }

        return Result.Success(responses);
    }

    private async Task<(string? BranchName, string? DepartmentName)> GetBranchAndDepartmentNamesAsync(
    Guid? branchId, Guid? departmentId, CancellationToken ct)
    {
        string? branchName = branchId is { } bId
            ? await _unitOfWork.Branches.Query().Where(b => b.Id == bId).Select(b => b.Name).FirstOrDefaultAsync(ct)
            : null;

        string? departmentName = departmentId is { } dId
            ? await _unitOfWork.Departments.Query().Where(d => d.Id == dId).Select(d => d.Name).FirstOrDefaultAsync(ct)
            : null;

        return (branchName, departmentName);
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == _currentUser.CompanyId, ct);

        if (user is null)
            return Result.Failure<UserResponse>(UserErrors.NotFound);

        var roles = await _userManager.GetRolesAsync(user);
        var (branchName, departmentName) = await GetBranchAndDepartmentNamesAsync(user.BranchId, user.DepartmentId, ct);

        return Result.Success(ToResponse(user, roles, branchName, departmentName));
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result.Failure<UserResponse>(UserErrors.EmailAlreadyExists);

        var validationError = await ValidateBranchAndDepartmentAsync(request.BranchId, request.DepartmentId, ct);
        if (validationError is not null)
            return Result.Failure<UserResponse>(validationError);

        var validRoles = await _roleManager.Roles.IgnoreQueryFilters()
            .Where(r => r.CompanyId == _currentUser.CompanyId && request.RoleNames.Contains(r.Name!))
            .Select(r => new { r.Id, r.Name })
            .ToListAsync(ct);

        if (validRoles.Count != request.RoleNames.Distinct().Count())
            return Result.Failure<UserResponse>(UserErrors.InvalidRoles);


        var user = new ApplicationUser
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            CompanyId = _currentUser.CompanyId,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            EmailConfirmed = true // admin-invited: trusted without the confirm-email round trip
        };

        // Random unusable password — the user sets their own via the invite email link below
        var tempPassword = $"Tmp!{Guid.NewGuid():N}A1";
        var createResult = await _userManager.CreateAsync(user, tempPassword);
        if (!createResult.Succeeded)
        {
            var error = createResult.Errors.First();
            return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        // NOTE: bypassing UserManager.AddToRolesAsync — it internally calls IsInRoleAsync,
        // which resolves roles by NormalizedName only and throws when multiple tenants
        // share the same role name (e.g. "Admin" per company). We already have the role
        // Ids from the validation query above, so insert the join rows directly.
        _context.UserRoles.AddRange(validRoles.Select(r => new IdentityUserRole<Guid> //  match your actual key type
        {
            UserId = user.Id,
            RoleId = r.Id
        }));
        await _context.SaveChangesAsync(ct);

        var validRoleNames = validRoles.Select(r => r.Name!).ToList();

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        try
        {
            await SendInviteEmail(user, code);
        }
        catch (FormatException)
        {
            return Result.Failure<UserResponse>(AuthErrors.FaliedToSendEmail);
        }

        var (branchName, departmentName) = await GetBranchAndDepartmentNamesAsync(request.BranchId, request.DepartmentId, ct);
        return Result.Success(ToResponse(user, validRoleNames, branchName, departmentName));
    }
    public async Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == _currentUser.CompanyId, ct);

        if (user is null)
            return Result.Failure<UserResponse>(UserErrors.NotFound);

        var validationError = await ValidateBranchAndDepartmentAsync(request.BranchId, request.DepartmentId, ct);
        if (validationError is not null)
            return Result.Failure<UserResponse>(validationError);


        user.FullName = request.FullName;
        user.BranchId = request.BranchId;
        user.DepartmentId = request.DepartmentId;
        user.IsActive = request.IsActive;

        await _userManager.UpdateAsync(user);

        var (branchName, departmentName) = await GetBranchAndDepartmentNamesAsync(user.BranchId, user.DepartmentId, ct);
        var roles = await _userManager.GetRolesAsync(user);
        return Result.Success(ToResponse(user, roles, branchName, departmentName));
    }

    public async Task<Result> AssignRolesAsync(Guid id, AssignUserRolesRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == _currentUser.CompanyId, ct);

        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        var validRoleNames = await _roleManager.Roles.IgnoreQueryFilters()
            .Where(r => r.CompanyId == _currentUser.CompanyId && request.RoleNames.Contains(r.Name!))
            .Select(r => r.Name!)
            .ToListAsync(ct);

        if (validRoleNames.Count != request.RoleNames.Distinct().Count())
            return Result.Failure(UserErrors.InvalidRoles);

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, validRoleNames);

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == _currentUser.CompanyId, ct);

        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        user.IsActive = false;
        user.RefreshTokens.ForEach(rt => rt.RevokedIn = DateTime.UtcNow); // kill any active sessions immediately

        await _userManager.UpdateAsync(user);
        return Result.Success();
    }

    private static UserResponse ToResponse(ApplicationUser user, IEnumerable<string> roles, string? branchName, string? departmentName) => new(
    user.Id, user.Email!, user.FullName, user.BranchId, branchName, user.DepartmentId, departmentName, user.IsActive, roles.ToList());

    private async Task SendInviteEmail(ApplicationUser user, string code)
    {
        var emailBody = EmailBodyBuilder.GenerateEmailBody(_webHostEnvironment.ContentRootPath,
            "ForgetPasswordTemplate", new Dictionary<string, string>
            {
                { "{{name}}", user.FullName },
                { "{{action_url}}", $"{_domainOptions.Domain1}/auth/forgetPassword?email={Uri.EscapeDataString(user.Email!)}&code={code}" }
            });

        await _emailService.SendEmailAsync(user.Email!, "You've been invited to Synaptech ERP — set your password", emailBody);
    }
}