using FluentValidation;

public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Warehouse name is required").MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().WithMessage("Warehouse code is required").MaximumLength(30);
    }
}
