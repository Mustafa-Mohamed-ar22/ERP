using FluentValidation;

public class VoidCashierOrderRequestValidator : AbstractValidator<VoidCashierOrderRequest>
{
    public VoidCashierOrderRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
    }
}