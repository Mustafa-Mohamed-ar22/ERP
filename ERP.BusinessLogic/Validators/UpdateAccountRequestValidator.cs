using FluentValidation;

public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Account code is required").MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Account name is required").MaximumLength(200);
    }
}
