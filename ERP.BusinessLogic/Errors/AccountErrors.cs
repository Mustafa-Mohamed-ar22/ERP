using Microsoft.AspNetCore.Http;

public static class AccountErrors
{
    public static readonly Error NotFound = new(
        "Account.NotFound", "Account not found", "الحساب غير موجود", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateCode = new(
        "Account.DuplicateCode", "An account with this code already exists",
        "يوجد حساب بنفس الكود بالفعل", StatusCodes.Status409Conflict);

    public static readonly Error ParentNotFound = new(
        "Account.ParentNotFound", "The specified parent account was not found",
        "الحساب الرئيسي المحدد غير موجود", StatusCodes.Status400BadRequest);

    public static readonly Error ParentTypeMismatch = new(
        "Account.ParentTypeMismatch", "A sub-account must have the same account type as its parent",
        "يجب أن يكون نوع الحساب الفرعي مطابقًا لنوع الحساب الرئيسي", StatusCodes.Status400BadRequest);

    public static readonly Error HasChildrenOrTransactions = new(
        "Account.HasChildrenOrTransactions", "This account has sub-accounts or transactions and cannot be deleted",
        "هذا الحساب يحتوي على حسابات فرعية أو حركات ولا يمكن حذفه", StatusCodes.Status400BadRequest);

    public static readonly Error NotPostable = new(
        "Account.NotPostable", "Journal lines can only post to accounts with no sub-accounts",
        "لا يمكن الترحيل إلا إلى الحسابات التي ليس لها حسابات فرعية", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidPeriod = new(
        "Account.InvalidPeriod", "Period end must be on or after period start",
        "يجب أن يكون تاريخ نهاية الفترة بعد أو يساوي تاريخ البداية", StatusCodes.Status400BadRequest);

    public static readonly Error CashAccountNotConfigured = new(
        "Account.CashAccountNotConfigured", "Cash account is not configured in Accounting Settings",
        "لم يتم تكوين حساب النقدية في إعدادات المحاسبة", StatusCodes.Status400BadRequest);
}
