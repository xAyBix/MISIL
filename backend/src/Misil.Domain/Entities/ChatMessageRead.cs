namespace Misil.Domain.Entities;

public class ChatMessageRead
{
    public Guid UserId { get; set; }
    public Guid MessageId { get; set; }
    public DateTime ReadAt { get; set; }

    public ChatMessageRead() { }

    public ChatMessageRead(Guid userId, Guid messageId)
    {
        UserId = userId;
        MessageId = messageId;
        ReadAt = DateTime.UtcNow;
    }
}
