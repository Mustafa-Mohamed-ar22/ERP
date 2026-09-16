using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEmailSender _emailService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly DomainCORS _domainOptions;
    private readonly ApplicationDbContext _context;
    public EmployeeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
        IEmailSender emailService, IWebHostEnvironment webHostEnvironment, IOptions<DomainCORS> options, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _roleManager = roleManager;
        _userManager = userManager;
        _emailService = emailService;
        _webHostEnvironment = webHostEnvironment;
        _domainOptions = options.Value;
        _context = context;
    }

    public async Task<Result<List<EmployeeResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var employees = await _unitOfWork.Employees.Query().ToListAsync(ct);
        return Result.Success(employees.Select(ToResponse).ToList());
    }

    public async Task<Result<EmployeeResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, ct);
        if (employee is null)
            return Result.Failure<EmployeeResponse>(EmployeeErrors.NotFound);

        return Result.Success(ToResponse(employee));
    }

    public async Task<Result<EmployeeResponse>> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var duplicateExists = await _unitOfWork.Employees.Query().AnyAsync(e => e.EmployeeCode == request.EmployeeCode, ct);
        if (duplicateExists)
            return Result.Failure<EmployeeResponse>(EmployeeErrors.DuplicateCode);

        if(request.Email is not null)
        {
            var existingEmployeeEmail = await _unitOfWork.Employees.Query()
            .AnyAsync(c => c.Email == request.Email, ct);
            if (existingEmployeeEmail)
                return Result.Failure<EmployeeResponse>(EmployeeErrors.EmailAlreadyExists);
        }
        if(request.Phone is not null)
        {
            var existingCustomerPhone = await _unitOfWork.Employees.Query()
            .AnyAsync(c => c.Phone == request.Phone, ct);
            if (existingCustomerPhone)
                return Result.Failure<EmployeeResponse>(EmployeeErrors.PhoneAlreadyExists);
        }
        


        if (request.DepartmentId is { } departmentId)
        {
            var Deptexists = await _unitOfWork.Departments.Query().AnyAsync(d => d.Id == departmentId, ct);
            if (!Deptexists) return Result.Failure<EmployeeResponse>(EmployeeErrors.DepartmentNotFound);
        }

        if (request.BranchId is { } branchId)
        {
            var exists = await _unitOfWork.Branches.Query().AnyAsync(b => b.Id == branchId, ct);
            if (!exists) return Result.Failure<EmployeeResponse>(EmployeeErrors.BranchNotFound);
        }
        if(request.BranchId is not null && request.DepartmentId is not null)
        {
            var isDeptBelongsToBranch = await _unitOfWork.Departments.Query()
                    .Where(d => d.Id == request.DepartmentId && d.BranchId == request.BranchId)
                    .AnyAsync(ct);
            if (!isDeptBelongsToBranch)
                return Result.Failure<EmployeeResponse>(EmployeeErrors.DepartmentNotBelongsToBranch);
        }
        if (request.ManagerId is { } managerId)
        {
            var exists = await _unitOfWork.Employees.Query().AnyAsync(e => e.Id == managerId, ct);
            if (!exists) return Result.Failure<EmployeeResponse>(EmployeeErrors.ManagerNotFound);
        }

        var employee = new Employee
        {
            CompanyId = _currentUser.CompanyId,
            EmployeeCode = request.EmployeeCode,
            FullName = request.FullName,
            NationalId = request.NationalId,
            DateOfBirth = request.DateOfBirth,
            HireDate = request.HireDate,
            JobTitle = request.JobTitle,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId,
            ManagerId = request.ManagerId,
            BaseSalary = request.BaseSalary,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = EmploymentStatus.Active,
            UserId = request.UserId
        };

        try
        {
            await _unitOfWork.Employees.AddAsync(employee, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Result.Failure<EmployeeResponse>(EmployeeErrors.DuplicateCode);
        }

        return Result.Success(ToResponse(employee));
    }

    public async Task<Result<UserResponse>> GrantAccessAsync(Guid employeeId, GrantEmployeeAccessRequest request, CancellationToken ct = default)
    {
        var roleNames = request.RoleNames ?? new List<string>();

        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId, ct);
        if (employee is null)
            return Result.Failure<UserResponse>(EmployeeErrors.NotFound);

        if (employee.UserId is not null)
            return Result.Failure<UserResponse>(EmployeeErrors.AlreadyHasAccess);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result.Failure<UserResponse>(UserErrors.EmailAlreadyExists);

        var validRoles = await _roleManager.Roles.IgnoreQueryFilters()
            .Where(r => r.CompanyId == _currentUser.CompanyId && roleNames.Contains(r.Name!))
            .Select(r => new { r.Id, r.Name })
            .ToListAsync(ct);

        if (validRoles.Count != roleNames.Distinct().Count())
            return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

        // Copied FROM the employee — no separate typed-in values that could disagree.
        var user = new ApplicationUser
        {
            FullName = employee.FullName,
            Email = request.Email,
            UserName = request.Email,
            CompanyId = _currentUser.CompanyId,
            BranchId = employee.BranchId,
            DepartmentId = employee.DepartmentId,
            IsActive = true,
            EmailConfirmed = true
        };

        // Random unusable password — the user sets their own via the invite email link below
        var tempPassword = $"Tmp!{Guid.NewGuid():N}A1";
        var createResult = await _userManager.CreateAsync(user, tempPassword);
        if (!createResult.Succeeded)
        {
            var error = createResult.Errors.First();
            return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        var validRoleNames = validRoles.Select(r => r.Name!).ToList();

        // NOTE: bypassing UserManager.AddToRolesAsync — it resolves roles by NormalizedName only and
        // becomes ambiguous once multiple companies share a role name (e.g. "Admin" per company).
        // We already have the exact RoleIds from the validation query above, so insert the join rows directly.
        _context.UserRoles.AddRange(validRoles.Select(r => new IdentityUserRole<Guid> // match your actual key type
        {
            UserId = user.Id,
            RoleId = r.Id
        }));
        await _context.SaveChangesAsync(ct);

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        try
        {
            await SendInviteEmail(user, code); // same helper UserService.CreateAsync already has
        }
        catch (FormatException)
        {
            return Result.Failure<UserResponse>(AuthErrors.FaliedToSendEmail);
        }

        employee.UserId = user.Id;
        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(ct);

        var branchName = employee.BranchId is not null
            ? await _unitOfWork.Branches.Query().Where(b => b.Id == employee.BranchId).Select(b => b.Name).FirstOrDefaultAsync(ct)
            : null;
        var departmentName = employee.DepartmentId is not null
            ? await _unitOfWork.Departments.Query().Where(d => d.Id == employee.DepartmentId).Select(d => d.Name).FirstOrDefaultAsync(ct)
            : null;

        return Result.Success(new UserResponse(
            user.Id, user.Email!, user.FullName, user.BranchId, branchName, user.DepartmentId, departmentName,
            user.IsActive, validRoleNames));
    }
    public async Task<Result<EmployeeResponse>> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, ct);
        if (employee is null)
            return Result.Failure<EmployeeResponse>(EmployeeErrors.NotFound);

        if (!Enum.TryParse<EmploymentStatus>(request.Status, true, out var status))
            return Result.Failure<EmployeeResponse>(new Error("Employee.InvalidStatus", "Invalid employment status", "???? ??????? ??? ?????", StatusCodes.Status400BadRequest));

        if (request.DepartmentId is { } departmentId)
        {
            var exists = await _unitOfWork.Departments.Query().AnyAsync(d => d.Id == departmentId, ct);
            if (!exists) return Result.Failure<EmployeeResponse>(EmployeeErrors.DepartmentNotFound);
        }

        if (request.BranchId is { } branchId)
        {
            var exists = await _unitOfWork.Branches.Query().AnyAsync(b => b.Id == branchId, ct);
            if (!exists) return Result.Failure<EmployeeResponse>(EmployeeErrors.BranchNotFound);
        }

        if (request.ManagerId is { } managerId)
        {
            if (managerId == id)
                return Result.Failure<EmployeeResponse>(EmployeeErrors.InvalidManager);

            var exists = await _unitOfWork.Employees.Query().AnyAsync(e => e.Id == managerId, ct);
            if (!exists) return Result.Failure<EmployeeResponse>(EmployeeErrors.ManagerNotFound);

            var currentAncestorId = (Guid?)managerId;
            var depth = 0;
            while (currentAncestorId is not null && depth < 50)
            {
                if (currentAncestorId == id)
                    return Result.Failure<EmployeeResponse>(EmployeeErrors.InvalidManager);

                currentAncestorId = await _unitOfWork.Employees.Query()
                    .Where(e => e.Id == currentAncestorId)
                    .Select(e => e.ManagerId)
                    .FirstOrDefaultAsync(ct);
                depth++;
            }
        }
        if(request.Email is not null)
        {
            var existingEmployeeEmail = await _unitOfWork.Employees.Query()
        .AnyAsync(c => c.Id != id && c.Email == request.Email, ct);
            if (existingEmployeeEmail)
                return Result.Failure<EmployeeResponse>(EmployeeErrors.EmailAlreadyExists);
        }
        if(request.Phone is not null)
        {
            var existingCustomerPhone = await _unitOfWork.Employees.Query()
         .AnyAsync(c => c.Id != id && c.Phone == request.Phone, ct);
            if (existingCustomerPhone)
                return Result.Failure<EmployeeResponse>(EmployeeErrors.PhoneAlreadyExists);
        }
        employee.FullName = request.FullName;
        employee.NationalId = request.NationalId;
        employee.DateOfBirth = request.DateOfBirth;
        employee.JobTitle = request.JobTitle;
        employee.DepartmentId = request.DepartmentId;
        employee.BranchId = request.BranchId;
        employee.ManagerId = request.ManagerId;
        employee.BaseSalary = request.BaseSalary;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.Address = request.Address;
        employee.Status = status;
        employee.UserId = request.UserId;

        _unitOfWork.Employees.Update(employee);
        if (employee.UserId is { } userId)
        {
            var linkedUser = await _userManager.FindByIdAsync(userId.ToString());
            if (linkedUser is not null)
            {
                linkedUser.FullName = employee.FullName;
                linkedUser.BranchId = employee.BranchId;
                linkedUser.DepartmentId = employee.DepartmentId;
                await _userManager.UpdateAsync(linkedUser);
            }
        }
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(employee));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, ct);
        if (employee is null)
            return Result.Failure(EmployeeErrors.NotFound);

        var hasDirectReports = await _unitOfWork.Employees.Query().AnyAsync(e => e.ManagerId == id, ct);
        var hasLeaveRequests = await _unitOfWork.LeaveRequests.Query().AnyAsync(l => l.EmployeeId == id, ct);
        var hasAttendance = await _unitOfWork.AttendanceRecords.Query().AnyAsync(a => a.EmployeeId == id, ct);

        if (hasDirectReports || hasLeaveRequests || hasAttendance)
            return Result.Failure(EmployeeErrors.HasDirectReportsOrRecords);

        employee.IsDeleted = true;
        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static EmployeeResponse ToResponse(Employee e) => new(
        e.Id, e.EmployeeCode, e.FullName, e.NationalId, e.DateOfBirth, e.HireDate, e.JobTitle,
        e.DepartmentId, e.BranchId, e.ManagerId, e.BaseSalary, e.Email, e.Phone, e.Address, e.Status.ToString(), e.UserId);
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