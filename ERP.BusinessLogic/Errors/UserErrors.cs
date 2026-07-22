using Microsoft.AspNetCore.Http;

public static class UserErrors
{
    public static readonly Error NotFound = new(
        "User.NotFound", "User not found", "المستخدم غير موجود", StatusCodes.Status404NotFound);

    public static readonly Error EmailAlreadyExists = new(
        "User.EmailAlreadyExists", "An account with this email already exists",
        "يوجد حساب مسجل بهذا البريد الإلكتروني بالفعل", StatusCodes.Status409Conflict);

    public static readonly Error InvalidRoles = new(
        "User.InvalidRoles", "One or more specified roles do not exist for this company",
        "دور واحد أو أكثر من الأدوار المحددة غير موجود لهذه الشركة", StatusCodes.Status400BadRequest);

    public static readonly Error BranchNotFound = new(
        "User.BranchNotFound", "The specified branch was not found",
        "الفرع المحدد غير موجود", StatusCodes.Status400BadRequest);

    public static readonly Error DepartmentNotFound = new(
        "User.DepartmentNotFound", "The specified department was not found",
        "الإدارة المحددة غير موجودة", StatusCodes.Status400BadRequest);

    public static readonly Error DepartmentBranchMismatch = new(
        "User.DepartmentBranchMismatch", "The specified department does not belong to the specified branch",
        "الإدارة المحددة لا تتبع الفرع المحدد", StatusCodes.Status400BadRequest);
}