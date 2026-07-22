using Microsoft.AspNetCore.Http;

public static class BranchErrors
{
    public static readonly Error NotFound = new(
        "Branch.NotFound", "Branch not found", "الفرع غير موجود", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateCode = new(
        "Branch.DuplicateCode", "A branch with this code already exists",
        "يوجد فرع بنفس الكود بالفعل", StatusCodes.Status409Conflict);
}