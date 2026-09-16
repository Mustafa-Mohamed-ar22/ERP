using FluentValidation;

public class ShipGoodsRequestValidator : AbstractValidator<ShipGoodsRequest>
{
    public ShipGoodsRequestValidator()
    {
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.LineId).NotEmpty();
            line.RuleFor(l => l.QuantityShipped).GreaterThanOrEqualTo(0).WithMessage("Shipped quantity must be greater than zero");
        });
    }
}