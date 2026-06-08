namespace Misil.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsRead { get; set; }

    public User User { get; set; } = null!;

    public ChatMessage() { }

    public ChatMessage(Guid channelId, Guid userId, string content)
    {
        Id = Guid.NewGuid();
        ChannelId = channelId;
        UserId = userId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }
}
