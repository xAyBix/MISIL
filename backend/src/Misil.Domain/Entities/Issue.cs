using Misil.Domain.Enums;

namespace Misil.Domain.Entities;

public class Issue
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ParentIssueId { get; set; }
    public IssueType IssueType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.ToDo;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    public Guid ReporterId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? AssigneeTeamId { get; set; }
    public int? StoryPoints { get; set; }
    public int Order { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public User Reporter { get; set; } = null!;
    public User? Assignee { get; set; }
    public Issue? ParentIssue { get; set; }
    public ICollection<Issue> Subtasks { get; set; } = new List<Issue>();
    public ICollection<IssueComment> Comments { get; set; } = new List<IssueComment>();
    public ICollection<IssueAttachment> Attachments { get; set; } = new List<IssueAttachment>();
    public ICollection<IssueRelation> RelatedIssues { get; set; } = new List<IssueRelation>();
    public ICollection<IssueDependency> Dependencies { get; set; } = new List<IssueDependency>();
    public ICollection<SprintIssue> SprintIssues { get; set; } = new List<SprintIssue>();

    public Issue() { }

    public Issue(Guid projectId, IssueType issueType, string title, Guid reporterId, string? description = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        IssueType = issueType;
        Title = title;
        Description = description;
        ReporterId = reporterId;
        Status = IssueStatus.ToDo;
        Priority = IssuePriority.Medium;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
