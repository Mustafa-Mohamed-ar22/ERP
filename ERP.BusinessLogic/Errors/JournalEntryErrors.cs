using Microsoft.AspNetCore.Http;

public static class JournalEntryErrors
{
    public static readonly Error NotFound = new(
        "JournalEntry.NotFound", "Journal entry not found", "القيد غير موجود", StatusCodes.Status404NotFound);

    public static readonly Error Unbalanced = new(
        "JournalEntry.Unbalanced", "Total debit must equal total credit",
        "يجب أن يتساوى إجمالي المدين مع إجمالي الدائن", StatusCodes.Status400BadRequest);

    public static readonly Error EmptyLines = new(
        "JournalEntry.EmptyLines", "A journal entry must have at least two lines",
        "يجب أن يحتوي القيد على سطرين على الأقل", StatusCodes.Status400BadRequest);

    public static readonly Error AlreadyPosted = new(
        "JournalEntry.AlreadyPosted", "This journal entry is already posted",
        "تم ترحيل هذا القيد بالفعل", StatusCodes.Status400BadRequest);

    public static readonly Error NotDraft = new(
        "JournalEntry.NotDraft", "Only draft entries can be edited or deleted",
        "يمكن تعديل أو حذف القيود في حالة المسودة فقط", StatusCodes.Status400BadRequest);

    public static readonly Error NotPosted = new(
        "JournalEntry.NotPosted", "Only posted entries can be reversed",
        "يمكن عكس القيود المرحلة فقط", StatusCodes.Status400BadRequest);

    public static readonly Error AlreadyReversed = new(
        "JournalEntry.AlreadyReversed", "This entry has already been reversed",
        "تم عكس هذا القيد بالفعل", StatusCodes.Status400BadRequest);
}
