using Microsoft.AspNetCore.Http;

public static class AttendanceErrors
{
    public static readonly Error NoLinkedEmployee = new("Attendance.NoLinkedEmployee", "Your account is not linked to an employee record", "حسابك غير مرتبط بسجل موظف", StatusCodes.Status400BadRequest);
    public static readonly Error AlreadyCheckedIn = new("Attendance.AlreadyCheckedIn", "Already checked in today", "تم تسجيل الحضور بالفعل اليوم", StatusCodes.Status400BadRequest);
    public static readonly Error NotCheckedIn = new("Attendance.NotCheckedIn", "Must check in before checking out", "يجب تسجيل الحضور أولاً قبل الانصراف", StatusCodes.Status400BadRequest);
    public static readonly Error AlreadyCheckedOut = new("Attendance.AlreadyCheckedOut", "Already checked out today", "تم تسجيل الانصراف بالفعل اليوم", StatusCodes.Status400BadRequest);
    public static readonly Error EmployeeNotActive = new(
        "Attendance.EmployeeNotActive", "This employee is not currently active",
        "هذا الموظف غير نشط حاليًا", StatusCodes.Status400BadRequest);
}