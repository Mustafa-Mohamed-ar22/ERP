using Microsoft.AspNetCore.Http;

public static class DepartmentErrors
{
    public static readonly Error NotFound = new(
        "Department.NotFound", "Department not found", "الإدارة غير موجودة", StatusCodes.Status404NotFound);

    public static readonly Error InvalidParent = new(
        "Department.InvalidParent", "A department cannot be its own ancestor",
        "لا يمكن أن تكون الإدارة تابعة لنفسها أو لأحد فروعها", StatusCodes.Status400BadRequest);

    public static readonly Error ParentNotFound = new(
        "Department.ParentNotFound", "The specified parent department was not found",
        "الإدارة الرئيسية المحددة غير موجودة", StatusCodes.Status400BadRequest);
}