using FluentValidation;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().WithMessage("SKU is required").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required").MaximumLength(200);
        RuleFor(x => x.UnitOfMeasure).NotEmpty().WithMessage("Unit of measure is required").MaximumLength(20);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
    }
}
