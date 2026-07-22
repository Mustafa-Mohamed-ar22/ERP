using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
namespace ERP.BusinessLogic.Services
{
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        JwtProvider jwtProvider,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IEmailSender emailService,
        IWebHostEnvironment webHostEnvironment,
        IOptions<DomainCORS> options) : IAuthService
    {
        private readonly IEmailSender _emailService = emailService;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly DomainCORS _domainOptions = options.Value;

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

            if (!user.IsActive)
                return Result.Failure<AuthResponse>(AuthErrors.InactiveUser);

            var result = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (result.Succeeded)
                return await BuildAuthResponseAsync(user);

            return Result.Failure<AuthResponse>(
                result.IsNotAllowed ? AuthErrors.EmailNotConfirmed : AuthErrors.InvalidCredentials);
        }

        public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                return Result.Failure<RegisterResponse>(AuthErrors.EmailAlreadyExists);

            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            try
            {
                var company = new Company { Name = request.CompanyName, Currency = "EGP", IsActive = true };
                context.Companies.Add(company);

                var mainBranch = new Branch { CompanyId = company.Id, Name = "Head Office", Code = "HQ", IsMain = true };
                context.Branches.Add(mainBranch);
                await context.SaveChangesAsync(ct);

                var user = new ApplicationUser
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    UserName = request.Email,
                    CompanyId = company.Id,
                    BranchId = mainBranch.Id,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync(ct);
                    var error = createResult.Errors.First();
                    return Result.Failure<RegisterResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
                }

                var adminRole = new ApplicationRole { Name = "Admin", CompanyId = company.Id, Description = "Company administrator" };
                var roleCreateResult = await roleManager.CreateAsync(adminRole);
                if (!roleCreateResult.Succeeded)
                {
                    await transaction.RollbackAsync(ct);
                    var error = roleCreateResult.Errors.First();
                    return Result.Failure<RegisterResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
                }

                context.UserRoles.Add(new IdentityUserRole<Guid> // or whatever your key type is
                {
                    UserId = user.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync(ct);
                var grantablePermissionIds = await context.Permissions.IgnoreQueryFilters()
                    .Where(p => !SystemPermissions.Codes.Contains(p.Code))
                    .Select(p => p.Id)
                    .ToListAsync(ct);

                context.RolePermissions.AddRange(
                    grantablePermissionIds.Select(permissionId => new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permissionId
                    }));

                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                try
                {
                    await SendEmail(user, code);
                }
                catch (FormatException)
                {
                    return Result.Failure<RegisterResponse>(AuthErrors.FaliedToSendEmail);
                }

                // No tokens issued here — the account exists but can't sign in until the email is confirmed.
                return Result.Success(new RegisterResponse(
                    user.Id, user.Email!, "Registration successful. Please check your email (or SPAM if not in main inbox) to confirm your account before logging in."));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default!)
        {
            if (await userManager.FindByIdAsync(request.UserId) is not { } user)
                return Result.Failure(AuthErrors.InvalideCode);

            if (user.EmailConfirmed)
                return Result.Failure(AuthErrors.AlreadyConfirmed);

            var code = request.Code;
            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Result.Failure(AuthErrors.InvalideCode);
            }

            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken ct = default!)
        {
            if (await userManager.FindByEmailAsync(request.Email) is not { } user)
                return Result.Success();

            if (user.EmailConfirmed)
                return Result.Failure(AuthErrors.AlreadyConfirmed);

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            try
            {
                await SendEmail(user, code);
            }
            catch (FormatException)
            {
                return Result.Failure(AuthErrors.FaliedToSendEmail);
            }

            return Result.Success();
        }

        public async Task<Result> SendResetPasswordCodeAsync(string email, CancellationToken ct = default!)
        {
            if (await userManager.FindByEmailAsync(email) is not { } user)
                return Result.Success(); // don't reveal whether the email is registered

            if (!user.EmailConfirmed)
                return Result.Failure(AuthErrors.EmailNotConfirmed);

            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            try
            {
                await SendResetPassword(user, code);
            }
            catch (FormatException)
            {
                return Result.Failure(AuthErrors.FaliedToSendEmail);
            }

            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default!)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.EmailConfirmed)
                return Result.Failure(AuthErrors.InvalideCode);

            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
                result = await userManager.ResetPasswordAsync(user, code, request.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
            }

            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var user = userManager.Users.SingleOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));
            if (user is null)
                return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

            var token = user.RefreshTokens.Single(x => x.Token == refreshToken);
            if (!token.IsActive)
                return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);

            token.RevokedIn = DateTime.UtcNow;

            var newRefreshToken = JwtProvider.GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            await userManager.UpdateAsync(user);

            return await BuildAuthResponseAsync(user, newRefreshToken.Token);
        }

        public async Task<Result> RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var user = userManager.Users.SingleOrDefault(u => u.RefreshTokens.Any(x => x.Token == refreshToken));
            if (user is null)
                return Result.Failure(AuthErrors.InvalidToken);

            var token = user.RefreshTokens.Single(x => x.Token == refreshToken);
            if (!token.IsActive)
                return Result.Failure(AuthErrors.InvalidToken);

            token.RevokedIn = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            return Result.Success();
        }

        private async Task<Result<AuthResponse>> BuildAuthResponseAsync(ApplicationUser user, string? existingRefreshToken = null)
        {
            var roles = await userManager.GetRolesAsync(user);
            var accessToken = jwtProvider.GenerateAccessTaoken(user, roles);

            string refreshTokenValue;
            if (existingRefreshToken is not null)
            {
                refreshTokenValue = existingRefreshToken;
            }
            else
            {
                var newRefreshToken = JwtProvider.GenerateRefreshToken();
                user.RefreshTokens.Add(newRefreshToken);
                await userManager.UpdateAsync(user);
                refreshTokenValue = newRefreshToken.Token;
            }

            return Result.Success(new AuthResponse(
                UserId: user.Id,
                Email: user.Email!,
                FullName: user.FullName,
                Role: roles.FirstOrDefault() ?? string.Empty,
                AccessToken: accessToken,
                RefreshToken: refreshTokenValue));
        }

        private async Task SendEmail(ApplicationUser user, string code)
        {
            var emailBody = EmailBodyBuilder.GenerateEmailBody(_webHostEnvironment.ContentRootPath,
                "TemplateSendEmail", new Dictionary<string, string>
                {
                    { "{{name}}", user.FullName },
                    { "{{action_url}}", $"{_domainOptions.Domain1}/auth/emailConfirmation?userId={user.Id}&code={code}" }
                    //{ "{{action_url}}", $"https://localhost:7061/api/auth/confirm-email?userId={user.Id}&code={code}" }
                });

            await _emailService.SendEmailAsync(user.Email!, "? Synaptech ERP: Verify your email", emailBody);
        }

        private async Task SendResetPassword(ApplicationUser user, string code)
        {
            var emailBody = EmailBodyBuilder.GenerateEmailBody(_webHostEnvironment.ContentRootPath,
                "ForgetPasswordTemplate", new Dictionary<string, string>
                {
                    { "{{name}}", user.FullName },
                    { "{{action_url}}", $"{_domainOptions.Domain1}/auth/forgetPassword?email={Uri.EscapeDataString(user.Email!)}&code={code}" }
                    //{ "{{action_url}}", $"https://localhost:7061/api/auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&code={code}" }
                });

            await _emailService.SendEmailAsync(user.Email!, "? Synaptech ERP: Reset your password", emailBody);
        }
    }
}