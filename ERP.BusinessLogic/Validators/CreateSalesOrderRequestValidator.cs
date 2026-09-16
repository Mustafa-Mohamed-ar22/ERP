using FluentValidation;

public class CreateSalesOrderRequestValidator : AbstractValidator<CreateSalesOrderRequest>
{
    public CreateSalesOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer is required");
        RuleFor(x => x.WarehouseId).NotEmpty().WithMessage("Warehouse is required");
        RuleFor(x => x.OrderDate).NotEqual(default(DateTime)).WithMessage("Order date is required");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line is required");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
            line.RuleFor(l => l.UnitPrice).GreaterThan(0);
        });
    }
}
