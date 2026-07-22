using Microsoft.AspNetCore.Http;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials", "Email or password is incorrect",
        "البريد الإلكتروني أو كلمة المرور غير صحيحة", StatusCodes.Status400BadRequest);

    public static readonly Error InactiveUser = new(
        "Auth.InactiveUser", "This user account is disabled",
        "هذا الحساب غير مفعل", StatusCodes.Status403Forbidden);

    public static readonly Error EmailNotConfirmed = new(
        "Auth.EmailNotConfirmed", "Please confirm your email before logging in",
        "يرجى تأكيد البريد الإلكتروني أولاً قبل تسجيل الدخول", StatusCodes.Status403Forbidden);

    public static readonly Error EmailAlreadyExists = new(
        "Auth.EmailAlreadyExists", "An account with this email already exists",
        "يوجد حساب مسجل بهذا البريد الإلكتروني بالفعل", StatusCodes.Status409Conflict);

    public static readonly Error FaliedToSendEmail = new(
        "Auth.FailedToSendEmail", "Failed to send the confirmation email",
        "فشل إرسال البريد الإلكتروني", StatusCodes.Status500InternalServerError);

    public static readonly Error InvalideCode = new(
        "Auth.InvalidCode", "The verification code is invalid or expired",
        "رمز التحقق غير صالح أو منتهي الصلاحية", StatusCodes.Status400BadRequest);

    public static readonly Error AlreadyConfirmed = new(
        "Auth.AlreadyConfirmed", "This email is already confirmed",
        "تم تأكيد هذا البريد الإلكتروني بالفعل", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidToken = new(
        "Auth.InvalidToken", "Refresh token is invalid or expired",
        "رمز التحديث غير صالح أو منتهي الصلاحية", StatusCodes.Status401Unauthorized);
}
