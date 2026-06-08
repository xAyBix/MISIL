using Misil.Domain.Enums;

namespace Misil.Application.Gantt;
public record GanttDataDto(List<GanttIssueDto> Issues, List<GanttDependencyDto> Dependencies);
public record GanttIssueDto(Guid Id, string Title, IssueType IssueType, IssueStatus Status, int Order, DateTime? StartDate, DateTime? DueDate, Guid? ParentIssueId);
public record GanttDependencyDto(Guid Id, Guid IssueId, Guid DependsOnIssueId, DependencyType DependencyType);
