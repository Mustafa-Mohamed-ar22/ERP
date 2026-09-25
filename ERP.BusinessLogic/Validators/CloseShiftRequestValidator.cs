using FluentValidation;

public class CloseShiftRequestValidator : AbstractValidator<CloseShiftRequest>
{
    public CloseShiftRequestValidator()
    {
        RuleFor(x => x.CountedClosingCash).GreaterThanOrEqualTo(0);
    }
}
