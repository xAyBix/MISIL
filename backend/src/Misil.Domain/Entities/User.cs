using Microsoft.AspNetCore.Identity;

namespace Misil.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    public ICollection<Issue> ReportedIssues { get; set; } = new List<Issue>();
    public ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
    public ICollection<IssueComment> Comments { get; set; } = new List<IssueComment>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public User() { }

    public User(string username, string email, string? displayName = null)
    {
        Id = Guid.NewGuid();
        UserName = username;
        Email = email;
        DisplayName = displayName;
        CreatedAt = DateTime.UtcNow;
    }
}
