namespace Misil.Application.Sprints;

using Misil.Domain.Enums;

public record SprintDto(Guid Id, string Name, string? Goal, Guid ProjectId, bool IsActive, bool IsCompleted, DateTime? StartDate, DateTime? EndDate, int IssueCount, int TotalStoryPoints, int CompletedStoryPoints, DateTime CreatedAt)
{
    public static SprintDto FromSprint(Domain.Entities.Sprint s) => new(
        s.Id, s.Name, s.Goal, s.ProjectId, s.IsActive, s.IsCompleted, s.StartDate, s.EndDate,
        s.SprintIssues.Count,
        s.SprintIssues.Sum(si => si.Issue.StoryPoints ?? 0),
        s.SprintIssues.Where(si => si.Issue.Status == IssueStatus.Done).Sum(si => si.Issue.StoryPoints ?? 0),
        s.CreatedAt
    );
}
