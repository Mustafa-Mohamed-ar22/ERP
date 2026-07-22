// Validators/ConfirmEmailRequestValidator.cs (ERP.BusinessLogic/Validators)
using FluentValidation;
// Validators/ConfirmEmailRequestValidator.cs
public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Confirmation code is required");
    }
}
