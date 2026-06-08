using Misil.Domain.Enums;

namespace Misil.Domain.Entities;

public class IssueDependency
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public Guid DependsOnIssueId { get; set; }
    public DependencyType DependencyType { get; set; }

    public Issue Issue { get; set; } = null!;
    public Issue DependsOnIssue { get; set; } = null!;

    public IssueDependency() { }

    public IssueDependency(Guid issueId, Guid dependsOnIssueId, DependencyType dependencyType)
    {
        Id = Guid.NewGuid();
        IssueId = issueId;
        DependsOnIssueId = dependsOnIssueId;
        DependencyType = dependencyType;
    }
}
