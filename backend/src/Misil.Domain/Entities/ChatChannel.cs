namespace Misil.Domain.Entities;

public class ChatChannel
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDirectMessage { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreatedAt { get; set; }

    public Project Project { get; set; } = null!;

    public ChatChannel() { }

    public ChatChannel(Guid projectId, string name, Guid? teamId, bool isPrivate)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = name;
        TeamId = teamId;
        IsPrivate = isPrivate;
        CreatedAt = DateTime.UtcNow;
    }
}
