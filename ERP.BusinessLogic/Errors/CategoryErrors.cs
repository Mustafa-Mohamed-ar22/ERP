using Microsoft.AspNetCore.Http;

public static class CategoryErrors
{
    public static readonly Error NotFound = new(
        "Category.NotFound", "Category not found", "الفئة غير موجودة", StatusCodes.Status404NotFound);

    public static readonly Error InvalidParent = new(
        "Category.InvalidParent", "A category cannot be its own ancestor",
        "لا يمكن أن تكون الفئة تابعة لنفسها أو لأحد فروعها", StatusCodes.Status400BadRequest);

    public static readonly Error ParentNotFound = new(
        "Category.ParentNotFound", "The specified parent category was not found",
        "الفئة الرئيسية المحددة غير موجودة", StatusCodes.Status400BadRequest);

    public static readonly Error HasChildrenOrProducts = new(
        "Category.HasChildrenOrProducts", "This category has sub-categories or products and cannot be deleted",
        "هذه الفئة تحتوي على فئات فرعية أو منتجات ولا يمكن حذفها", StatusCodes.Status400BadRequest);
    public static readonly Error DuplicateName = new(
        "Category.DuplicateName", "A category with name already existed",
        "يوجد بالفعل فئة منتجات بهذا الاسم", StatusCodes.Status400BadRequest);
}