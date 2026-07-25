using FluentValidation;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Account code is required").MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Account name is required").MaximumLength(200);
        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("Account type is required")
            .Must(t => Enum.TryParse<AccountType>(t, true, out _))
            .WithMessage("Account type must be one of: Asset, Liability, Equity, Revenue, Expense");
    }
}
