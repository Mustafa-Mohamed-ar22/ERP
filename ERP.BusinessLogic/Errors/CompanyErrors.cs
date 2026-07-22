using Microsoft.AspNetCore.Http;

public static class CompanyErrors
{
    public static readonly Error NotFound = new(
        "Company.NotFound", "Company not found", "الشركة غير موجودة", StatusCodes.Status404NotFound);
}
