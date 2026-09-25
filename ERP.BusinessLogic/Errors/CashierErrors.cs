using Microsoft.AspNetCore.Http;

public static class CashierErrors
{
    public static readonly Error ShiftAlreadyOpen = new("Cashier.ShiftAlreadyOpen", "You already have an open shift for this warehouse", "لديك وردية مفتوحة بالفعل لهذا المخزن", StatusCodes.Status409Conflict);
    public static readonly Error NoOpenShift = new("Cashier.NoOpenShift", "No open shift found — open a shift before creating orders", "لا توجد وردية مفتوحة — يجب فتح وردية أولاً", StatusCodes.Status400BadRequest);
    public static readonly Error ShiftNotFound = new("Cashier.ShiftNotFound", "Shift not found", "الوردية غير موجودة", StatusCodes.Status404NotFound);
    public static readonly Error ShiftAlreadyClosed = new("Cashier.ShiftAlreadyClosed", "This shift is already closed", "هذه الوردية مغلقة بالفعل", StatusCodes.Status409Conflict);
    public static readonly Error WarehouseNotFound = WarehouseErrors.NotFound;
    public static readonly Error EmptyOrder = new("Cashier.EmptyOrder", "Order must contain at least one line", "يجب أن يحتوي الطلب على سطر واحد على الأقل", StatusCodes.Status400BadRequest);
    public static readonly Error ProductNotFound = ProductErrors.NotFound;
    public static readonly Error PaymentMismatch = new("Cashier.PaymentMismatch", "Total payments do not cover the order total", "إجمالي المدفوعات لا يغطي إجمالي الطلب", StatusCodes.Status400BadRequest);
    public static readonly Error CardOverpayment = new("Cashier.CardOverpayment", "Non-cash payments cannot exceed the order total — only cash produces change", "مدفوعات غير نقدية لا يمكن أن تتجاوز إجمالي الطلب — الباقي يُصرف نقدًا فقط", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidPaymentMethod = new("Cashier.InvalidPaymentMethod", "Payment method must be Cash, Card, or Other", "طريقة الدفع يجب أن تكون نقدًا أو بطاقة أو أخرى", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidMovementType = new("Cashier.InvalidMovementType", "Movement type must be CashIn or CashOut", "نوع الحركة يجب أن يكون إيداع أو سحب", StatusCodes.Status400BadRequest);
    public static readonly Error OrderNotFound = new("Cashier.OrderNotFound", "Order not found", "الطلب غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error OrderAlreadyVoided = new("Cashier.OrderAlreadyVoided", "This order is already voided", "تم إلغاء هذا الطلب بالفعل", StatusCodes.Status409Conflict);
    public static readonly Error CannotVoidAfterShiftClosed = new("Cashier.CannotVoidAfterShiftClosed", "Cannot void an order after its shift has been closed", "لا يمكن إلغاء الطلب بعد إغلاق الوردية الخاصة به", StatusCodes.Status409Conflict);
    public static readonly Error InvoiceNotFound = new("Cashier.InvoiceNotFound", "Invoice not found", "الفاتورة غير موجودة", StatusCodes.Status404NotFound);
    public static readonly Error PhoneRequired = new("Cashier.PhoneRequired", "A phone number is required to search", "رقم الهاتف مطلوب للبحث", StatusCodes.Status400BadRequest);
}