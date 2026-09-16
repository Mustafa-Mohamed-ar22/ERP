using Microsoft.AspNetCore.Http;

public static class LeaveRequestErrors
{
    public static readonly Error NotFound = new("LeaveRequest.NotFound", "Leave request not found", "طلب الإجازة غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error EmployeeNotFound = EmployeeErrors.NotFound;
    public static readonly Error InvalidDateRange = new("LeaveRequest.InvalidDateRange", "End date must be on or after start date", "يجب أن يكون تاريخ الانتهاء بعد أو يساوي تاريخ البدء", StatusCodes.Status400BadRequest);
    public static readonly Error OverlappingLeave = new("LeaveRequest.OverlappingLeave", "This employee already has an approved or pending leave request overlapping these dates", "يوجد للموظف طلب إجازة معتمد أو معلق يتداخل مع هذه التواريخ", StatusCodes.Status400BadRequest);
    public static readonly Error NotPending = new("LeaveRequest.NotPending", "Only pending leave requests can be approved, rejected, or cancelled", "يمكن فقط الموافقة أو الرفض أو الإلغاء للطلبات المعلقة", StatusCodes.Status400BadRequest);
    public static readonly Error NoLinkedEmployee = new(
    "LeaveRequest.NoLinkedEmployee", "Your account is not linked to an employee record",
    "حسابك غير مرتبط بسجل موظف", StatusCodes.Status400BadRequest);
}
