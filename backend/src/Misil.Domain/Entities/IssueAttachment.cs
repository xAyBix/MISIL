namespace Misil.Domain.Entities;

public class IssueAttachment
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }

    public Issue Issue { get; set; } = null!;
}
