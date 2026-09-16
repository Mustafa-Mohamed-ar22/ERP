using Microsoft.AspNetCore.Http;

public static class AccountingSettingsErrors
{
    public static readonly Error AccountTypeMismatch = new(
        "AccountingSettings.AccountTypeMismatch",
        "One or more accounts do not match the expected account type for that setting",
        "حساب واحد أو أكثر لا يتطابق مع نوع الحساب المتوقع لهذا الإعداد", StatusCodes.Status400BadRequest);
}