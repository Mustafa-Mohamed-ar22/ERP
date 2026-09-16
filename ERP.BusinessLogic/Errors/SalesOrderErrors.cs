using Microsoft.AspNetCore.Http;
public static class SalesOrderErrors
{
    public static readonly Error NotFound = new("SalesOrder.NotFound", "Sales order not found", "أمر البيع غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error LineNotFound = new("SalesOrder.LineNotFound", "Sales order line not found", "سطر أمر البيع غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error EmptyLines = new("SalesOrder.EmptyLines", "A sales order must have at least one line", "يجب أن يحتوي أمر البيع على سطر واحد على الأقل", StatusCodes.Status400BadRequest);
    public static readonly Error CustomerNotFound = CustomerErrors.NotFound;
    public static readonly Error WarehouseNotFound = WarehouseErrors.NotFound;
    public static readonly Error ProductNotFound = ProductErrors.NotFound;
    public static readonly Error InvalidStatusTransition = new("SalesOrder.InvalidStatusTransition", "This status transition is not allowed", "الانتقال بين هذه الحالات غير مسموح", StatusCodes.Status400BadRequest);
    public static readonly Error NotShippable = new("SalesOrder.NotShippable", "Only approved or partially shipped orders can be shipped", "يمكن الشحن فقط للأوامر المعتمدة أو المشحونة جزئيًا", StatusCodes.Status400BadRequest);
    public static readonly Error ShipQuantityExceedsRemaining = 
        new("SalesOrder.ShipQuantityExceedsRemaining", "Shipped quantity exceeds the remaining ordered quantity", "الكمية المشحونة تتجاوز الكمية المتبقية من الطلب", StatusCodes.Status400BadRequest);
    public static readonly Error CannotCancel = new("SalesOrder.CannotCancel", "Orders that have already shipped goods cannot be cancelled", "لا يمكن إلغاء الأوامر التي تم شحن بضائع منها", StatusCodes.Status400BadRequest);
    public static readonly Error NotDraft = new("SalesOrder.NotDraft", "Drafted Orders only can be updated", "فقط يمكن تعديل طلبات المبيعات الغير منشورة", StatusCodes.Status400BadRequest);
}