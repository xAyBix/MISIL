using Misil.Domain.Enums;

namespace Misil.Domain.Entities;

public class IssueRelation
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public Guid RelatedIssueId { get; set; }
    public RelationType RelationType { get; set; }

    public Issue Issue { get; set; } = null!;
}
