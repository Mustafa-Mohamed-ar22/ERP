// Entities/System/Setting.cs
// Entities/System/AuditLog.cs
// Entities/System/FileAttachment.cs
// Entities/System/FileAttachment.cs
public class FileAttachment : AuditableEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string FileName { get; set; } = default!;
    public string StoragePath { get; set; } = default!;   // blob/S3 key or local path
    public string ContentType { get; set; } = default!;
    public long SizeInBytes { get; set; }
    public string EntityName { get; set; } = default!;    // polymorphic attach point, e.g. "Proposal"
    public string EntityId { get; set; } = default!;
}