using Microsoft.AspNetCore.Http;

public static class ProductErrors
{
    public static readonly Error NotFound = new("Product.NotFound", "Product not found", "المنتج غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateSku = new("Product.DuplicateSku", "A product with this SKU already exists", "يوجد منتج بنفس الكود بالفعل", StatusCodes.Status409Conflict);
    public static readonly Error DuplicateName = new("Product.DuplicateName", "A product with this Name already exists", "يوجد منتج بنفس الاسم بالفعل", StatusCodes.Status409Conflict);
    public static readonly Error HasStockOrMovements = new("Product.HasStockOrMovements", "This product has stock or movement history and cannot be deleted", "هذا المنتج له مخزون أو حركات ولا يمكن حذفه", StatusCodes.Status400BadRequest);
}
