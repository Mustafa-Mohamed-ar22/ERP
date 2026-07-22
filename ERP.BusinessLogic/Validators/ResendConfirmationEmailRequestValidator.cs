using FluentValidation;
public class ResendConfirmationEmailRequestValidator : AbstractValidator<ResendConfirmationEmailRequest>
{
    public ResendConfirmationEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
