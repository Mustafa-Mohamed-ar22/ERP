using FluentValidation;

public class CreateCashierOrderRequestValidator : AbstractValidator<CreateCashierOrderRequest>
{
    public CreateCashierOrderRequestValidator()
    {
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.DiscountAmount).GreaterThanOrEqualTo(0);
        });
        RuleForEach(x => x.Payments).ChildRules(payment =>
        {
            payment.RuleFor(p => p.Method).Must(m => Enum.TryParse<CashierPaymentMethod>(m, true, out _))
                .WithMessage("Payment method must be Cash, Card, or Other");
            payment.RuleFor(p => p.Amount).GreaterThan(0);
        });

        RuleFor(x => x.WalkInCustomerPhone)
    .Matches(@"^(?:\+20|0020|0)?1[0125]\d{8}$")
    .When(x => x.WalkInCustomerPhone != null)
    .WithMessage("Invalid Egyptian phone number format. Example: +201012345678 or 01012345678");
    }
}
