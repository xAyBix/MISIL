using Misil.Domain.Enums;

namespace Misil.Application.Issues;
public record IssueDto(Guid Id, string Title, string? Description, IssueType IssueType, IssueStatus Status, IssuePriority Priority,
    Guid ProjectId, Guid? AssigneeId, Guid? AssigneeTeamId, Guid ReporterId, Guid? ParentIssueId, int Order,
    int? StoryPoints, DateTime? StartDate, DateTime? DueDate, DateTime CreatedAt, DateTime? UpdatedAt);

public record IssueDetailDto(Guid Id, string Title, string? Description, IssueType IssueType, IssueStatus Status, IssuePriority Priority,
    Guid ProjectId, Guid? AssigneeId, Guid? AssigneeTeamId, Guid ReporterId, Guid? ParentIssueId, int Order,
    int? StoryPoints, DateTime? StartDate, DateTime? DueDate, DateTime CreatedAt, DateTime? UpdatedAt,
    List<CommentDto> Comments, List<AttachmentDto> Attachments);

public record CommentDto(Guid Id, Guid IssueId, Guid UserId, string Content, DateTime CreatedAt);
public record AttachmentDto(Guid Id, Guid IssueId, string FileName, string FileUrl, Guid UploadedBy, DateTime UploadedAt);
