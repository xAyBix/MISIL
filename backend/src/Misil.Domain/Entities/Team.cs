namespace Misil.Domain.Entities;

public class Team
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<ChatChannel> Channels { get; set; } = new List<ChatChannel>();

    public Team() { }

    public Team(string name, string? description, Guid projectId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        ProjectId = projectId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AddMember(Guid userId) => Members.Add(new TeamMember { TeamId = Id, UserId = userId });

    public void RemoveMember(Guid userId)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member != null) Members.Remove(member);
    }
}
