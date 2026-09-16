using Microsoft.AspNetCore.Http;

public static class SupplierErrors
{
    public static readonly Error NotFound = new("Supplier.NotFound", "Supplier not found", "المورد غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error HasOrders = new("Supplier.HasOrders", "This supplier has purchase orders and cannot be deleted", "هذا المورد لديه أوامر شراء ولا يمكن حذفه", StatusCodes.Status400BadRequest);
    public static readonly Error EmailAlreadyExists = new("Supplier.EmailAlreadyExists", "supplier with email is already existed", "يوجد مورد بنفس الايميل", StatusCodes.Status400BadRequest);
    public static readonly Error PhoneAlreadyExists = new("Supplier.PhoneAlreadyExists", "supplier with phone is already existed", "يوجد مورد بنفس رقم الهاتف", StatusCodes.Status400BadRequest);
}
