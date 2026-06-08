namespace Misil.Domain.Entities;

public class IssueComment
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Issue Issue { get; set; } = null!;
    public User User { get; set; } = null!;
}
