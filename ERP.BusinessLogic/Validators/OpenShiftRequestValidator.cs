using FluentValidation;

public class OpenShiftRequestValidator : AbstractValidator<OpenShiftRequest>
{
    public OpenShiftRequestValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.OpeningCashBalance).GreaterThanOrEqualTo(0);
    }
}
