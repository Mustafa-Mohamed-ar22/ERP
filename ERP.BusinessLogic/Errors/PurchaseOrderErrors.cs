using Microsoft.AspNetCore.Http;

public static class PurchaseOrderErrors
{
    public static readonly Error NotFound = new("PurchaseOrder.NotFound", "Purchase order not found", "أمر الشراء غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error LineNotFound = new("PurchaseOrder.LineNotFound", "Purchase order line not found", "سطر أمر الشراء غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error EmptyLines = new("PurchaseOrder.EmptyLines", "A purchase order must have at least one line", "يجب أن يحتوي أمر الشراء على سطر واحد على الأقل", StatusCodes.Status400BadRequest);
    public static readonly Error SupplierNotFound = SupplierErrors.NotFound;
    public static readonly Error WarehouseNotFound = WarehouseErrors.NotFound;
    public static readonly Error ProductNotFound = ProductErrors.NotFound;
    public static readonly Error InvalidStatusTransition = new("PurchaseOrder.InvalidStatusTransition", "This status transition is not allowed", "الانتقال بين هذه الحالات غير مسموح", StatusCodes.Status400BadRequest);
    public static readonly Error NotReceivable = new("PurchaseOrder.NotReceivable", "Only approved or partially received orders can receive goods", "يمكن استلام البضائع فقط للأوامر المعتمدة أو المستلمة جزئيًا", StatusCodes.Status400BadRequest);
    public static readonly Error ReceiveQuantityExceedsRemaining = new("PurchaseOrder.ReceiveQuantityExceedsRemaining", "Received quantity exceeds the remaining ordered quantity", "الكمية المستلمة تتجاوز الكمية المتبقية من الطلب", StatusCodes.Status400BadRequest);
    public static readonly Error CannotCancel = new("PurchaseOrder.CannotCancel", "Orders that have already received goods cannot be cancelled", "لا يمكن إلغاء الأوامر التي تم استلام بضائع منها", StatusCodes.Status400BadRequest);
    public static readonly Error NotDraft = new("PurchaseOrder.NotDraft", "Drafted Orders only can be updated", "فقط يمكن تعديل الطلبات الغير منشورة", StatusCodes.Status400BadRequest);



}