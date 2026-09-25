using FluentValidation;

public class CashMovementRequestValidator : AbstractValidator<CashMovementRequest>
{
    public CashMovementRequestValidator()
    {
        RuleFor(x => x.MovementType).Must(t => Enum.TryParse<CashMovementType>(t, true, out _))
            .WithMessage("Movement type must be CashIn or CashOut");
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(250);
    }
}
