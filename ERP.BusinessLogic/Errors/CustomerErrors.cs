using Microsoft.AspNetCore.Http;

public static class CustomerErrors
{
    public static readonly Error NotFound = new("Customer.NotFound", "Customer not found", "العميل غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error HasOrders = new("Customer.HasOrders", "This customer has sales orders and cannot be deleted", "هذا العميل لديه أوامر بيع ولا يمكن حذفه", StatusCodes.Status400BadRequest);
    public static readonly Error EmailAlreadyExists = new("Customer.EmailAlreadyExists", "customer with email is already existed", "يوجد عميل بنفس الايميل", StatusCodes.Status400BadRequest);
    public static readonly Error PhoneAlreadyExists = new("Customer.PhoneAlreadyExists", "customer with phone is already existed", "يوجد عميل بنفس رقم الهاتف", StatusCodes.Status400BadRequest);
}