namespace Misil.Domain.Entities;

public class SprintIssue
{
    public Guid Id { get; set; }
    public Guid SprintId { get; set; }
    public Guid IssueId { get; set; }

    public Sprint Sprint { get; set; } = null!;
    public Issue Issue { get; set; } = null!;
}
