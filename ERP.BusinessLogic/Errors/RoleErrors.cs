using Microsoft.AspNetCore.Http;

public static class RoleErrors
{
    public static readonly Error NotFound = new(
        "Role.NotFound", "Role not found", "الدور غير موجود", StatusCodes.Status404NotFound);

    public static readonly Error InvalidPermissions = new(
        "Role.InvalidPermissions", "One or more specified permissions do not exist",
        "دور واحد أو أكثر من الصلاحيات المحددة غير موجود", StatusCodes.Status400BadRequest);

    public static readonly Error CannotDeleteRoleWithUsers = new(
        "Role.CannotDeleteRoleWithUsers", "This role has users assigned and cannot be deleted",
        "هذا الدور له مستخدمون مرتبطون به ولا يمكن حذفه", StatusCodes.Status400BadRequest);

}
