using Microsoft.AspNetCore.Http;

public static class EmployeeErrors
{
    public static readonly Error NotFound = new("Employee.NotFound", "Employee not found", "الموظف غير موجود", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateCode = new("Employee.DuplicateCode", "An employee with this code already exists", "يوجد موظف بنفس الكود بالفعل", StatusCodes.Status409Conflict);
    public static readonly Error InvalidManager = new("Employee.InvalidManager", "An employee cannot be their own manager, directly or indirectly", "لا يمكن أن يكون الموظف مديرًا لنفسه بشكل مباشر أو غير مباشر", StatusCodes.Status400BadRequest);
    public static readonly Error ManagerNotFound = new("Employee.ManagerNotFound", "The specified manager was not found", "المدير المحدد غير موجود", StatusCodes.Status400BadRequest);
    public static readonly Error DepartmentNotFound = DepartmentErrors.NotFound;
    public static readonly Error BranchNotFound = BranchErrors.NotFound;
    public static readonly Error HasDirectReportsOrRecords = new("Employee.HasDirectReportsOrRecords", "This employee has direct reports or HR records and cannot be deleted", "هذا الموظف لديه مرؤوسين أو سجلات موارد بشرية ولا يمكن حذفه", StatusCodes.Status400BadRequest);
    public static readonly Error DepartmentNotBelongsToBranch = new("Employee.DepartmentNotBelongsToBranch", "This department doesn't belong to this branch", "برجاء مراجعة البيانات .. هذا القسم لا ينتمى لهذا الفرع", StatusCodes.Status400BadRequest);
    public static readonly Error AlreadyHasAccess = new(
        "Employee.AlreadyHasAccess", "This employee already has system access",
        "هذا الموظف لديه وصول للنظام بالفعل", StatusCodes.Status400BadRequest);

    public static readonly Error EmailAlreadyExists = new("Employee.EmailAlreadyExists", "Employee with email is already existed", "يوجد موظف بنفس الايميل", StatusCodes.Status400BadRequest);
    public static readonly Error PhoneAlreadyExists = new("Employee.PhoneAlreadyExists", "Employee with phone is already existed", "يوجد موظف بنفس رقم الهاتف", StatusCodes.Status400BadRequest);
}
