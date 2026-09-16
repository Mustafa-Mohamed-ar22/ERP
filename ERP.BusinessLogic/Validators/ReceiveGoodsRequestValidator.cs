using FluentValidation;

public class ReceiveGoodsRequestValidator : AbstractValidator<ReceiveGoodsRequest>
{
    public ReceiveGoodsRequestValidator()
    {
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.LineId).NotEmpty();
            line.RuleFor(l => l.QuantityReceived).GreaterThanOrEqualTo(0).WithMessage("Received quantity must be greater than zero");
        });
    }
}