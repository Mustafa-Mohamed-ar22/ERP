using Microsoft.AspNetCore.Http;

public static class StockErrors
{
    public static readonly Error ProductNotFound = ProductErrors.NotFound;
    public static readonly Error WarehouseNotFound = WarehouseErrors.NotFound;

    public static readonly Error InvalidQuantity = new("Stock.InvalidQuantity", "Quantity must be greater than zero", "يجب أن تكون الكمية أكبر من صفر", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidMovementType = new("Stock.InvalidMovementType", "Invalid movement type — use In, Out, AdjustmentIncrease, or AdjustmentDecrease", "نوع الحركة غير صالح", StatusCodes.Status400BadRequest);
    public static readonly Error InsufficientStock = new("Stock.InsufficientStock", "Not enough stock available for this operation", "لا يوجد مخزون كافٍ لإتمام هذه العملية", StatusCodes.Status400BadRequest);
    public static readonly Error SameWarehouseTransfer = new("Stock.SameWarehouseTransfer", "Source and destination warehouse cannot be the same", "لا يمكن أن يكون مستودع المصدر والوجهة نفس المستودع", StatusCodes.Status400BadRequest);
    public static readonly Error ConcurrencyConflict = new("Stock.ConcurrencyConflict", "Stock was just updated by another operation — please retry", "تم تحديث المخزون من عملية أخرى، يرجى المحاولة مرة أخرى", StatusCodes.Status409Conflict);
}