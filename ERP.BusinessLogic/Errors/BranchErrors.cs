using Microsoft.AspNetCore.Http;

public static class BranchErrors
{
    public static readonly Error NotFound = new(
        "Branch.NotFound", "Branch not found", "الفرع غير موجود", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateCode = new(
        "Branch.DuplicateCode", "A branch with this code already exists",
        "يوجد فرع بنفس الكود بالفعل", StatusCodes.Status409Conflict);
    public static readonly Error DuplicateName = new(
        "Branch.DuplicateName", "A branch with this name already exists",
        "يوجد فرع بنفس الاسم بالفعل", StatusCodes.Status409Conflict);
}
