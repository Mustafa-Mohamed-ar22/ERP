using Microsoft.AspNetCore.Http;

public static class WarehouseErrors
{
    public static readonly Error NotFound = new("Warehouse.NotFound", "Warehouse not found", "المستودع غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateCode = new("Warehouse.DuplicateCode", "A warehouse with this code already exists", "يوجد مستودع بنفس الكود بالفعل", StatusCodes.Status409Conflict);
    public static readonly Error HasStock = new("Warehouse.HasStock", "This warehouse still has stock and cannot be deleted", "هذا المستودع يحتوي على مخزون ولا يمكن حذفه", StatusCodes.Status400BadRequest);
    public static readonly Error DuplicateName = new("Warehouse.DuplicateName", "A warehouse with this Name already exists", "يوجد مستودع بنفس الاسم بالفعل", StatusCodes.Status400BadRequest);
}
