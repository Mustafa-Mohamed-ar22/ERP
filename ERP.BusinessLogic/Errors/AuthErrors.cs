using Microsoft.AspNetCore.Http;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        code: "Auth.InvalidCredentials",
        errorDescription: "Email or password is incorrect",
        errorDescriptionAr: "البريد الإلكتروني أو كلمة المرور غير صحيحة",
        statusCode: StatusCodes.Status400BadRequest);

    public static readonly Error InvalidToken = new(
        code: "Auth.InvalidToken",
        errorDescription: "Refresh token is invalid or expired",
        errorDescriptionAr: "رمز التحديث غير صالح أو منتهي الصلاحية",
        statusCode: StatusCodes.Status401Unauthorized);

    public static readonly Error UserInactive = new(
        code: "Auth.UserInactive",
        errorDescription: "This user account is disabled",
        errorDescriptionAr: "هذا الحساب غير مفعل",
        statusCode: StatusCodes.Status403Forbidden);
}