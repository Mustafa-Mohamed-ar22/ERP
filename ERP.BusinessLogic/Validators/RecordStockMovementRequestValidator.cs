using FluentValidation;

public class RecordStockMovementRequestValidator : AbstractValidator<RecordStockMovementRequest>
{
    public RecordStockMovementRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
        RuleFor(x => x.MovementType)
            .NotEmpty()
            .Must(t => Enum.TryParse<StockMovementType>(t, true, out var parsed)
                       && parsed is StockMovementType.In or StockMovementType.Out
                           or StockMovementType.AdjustmentIncrease or StockMovementType.AdjustmentDecrease)
            .WithMessage("Movement type must be one of: In, Out, AdjustmentIncrease, AdjustmentDecrease");
    }
}
