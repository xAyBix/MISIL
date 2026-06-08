namespace Misil.Domain.Entities;

public class Sprint
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public ICollection<SprintIssue> SprintIssues { get; set; } = new List<SprintIssue>();

    public Sprint() { }

    public Sprint(string name, string? goal, Guid projectId, DateTime? startDate, DateTime? endDate)
    {
        Id = Guid.NewGuid();
        Name = name;
        Goal = goal;
        ProjectId = projectId;
        StartDate = startDate;
        EndDate = endDate;
        CreatedAt = DateTime.UtcNow;
    }
}
