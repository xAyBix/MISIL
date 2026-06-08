namespace Misil.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Key { get; set; } = string.Empty;
    public Guid LeadUserId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<ChatChannel> Channels { get; set; } = new List<ChatChannel>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public Project() { }

    public Project(string name, string? description, string key, Guid leadUserId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Key = key.ToUpperInvariant();
        LeadUserId = leadUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMember(Guid userId, Guid? roleId)
    {
        Members.Add(new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = Id,
            UserId = userId,
            RoleId = roleId ?? Guid.Empty,
            JoinedAt = DateTime.UtcNow
        });
    }
}
