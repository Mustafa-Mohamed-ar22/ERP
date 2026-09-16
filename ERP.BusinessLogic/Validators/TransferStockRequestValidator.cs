using FluentValidation;

public class TransferStockRequestValidator : AbstractValidator<TransferStockRequest>
{
    public TransferStockRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FromWarehouseId).NotEmpty();
        RuleFor(x => x.ToWarehouseId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
        RuleFor(x => x).Must(x => x.FromWarehouseId != x.ToWarehouseId)
            .WithMessage("Source and destination warehouse must be different");
    }
}