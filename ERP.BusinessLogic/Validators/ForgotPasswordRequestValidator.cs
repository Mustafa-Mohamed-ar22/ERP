// Validators/ForgotPasswordRequestValidator.cs (ERP.BusinessLogic/Validators)
using FluentValidation;
// Validators/ForgotPasswordRequestValidator.cs
// (see note below the code — recommend wrapping the raw `string email` param in a DTO)
public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
