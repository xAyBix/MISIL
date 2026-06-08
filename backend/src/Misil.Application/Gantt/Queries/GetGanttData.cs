using MediatR;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Gantt.Queries;
public record GetGanttDataQuery(Guid ProjectId) : IRequest<GanttDataDto>;

public class GetGanttDataQueryHandler : IRequestHandler<GetGanttDataQuery, GanttDataDto>
{
    private readonly IIssueRepository _issueRepo;
    private readonly IGanttRepository _ganttRepo;

    public GetGanttDataQueryHandler(IIssueRepository issueRepo, IGanttRepository ganttRepo)
    {
        _issueRepo = issueRepo;
        _ganttRepo = ganttRepo;
    }

    public async Task<GanttDataDto> Handle(GetGanttDataQuery request, CancellationToken ct)
    {
        var issues = await _issueRepo.GetFilteredAsync(request.ProjectId, null, null, null, null, null, ct);
        var withDates = issues.Where(i => i.StartDate != null || i.DueDate != null).ToList();
        var dependencies = await _ganttRepo.GetDependenciesAsync(request.ProjectId, ct);

        return new GanttDataDto(
            withDates.Select(i => new GanttIssueDto(i.Id, i.Title, i.IssueType, i.Status, i.Order, i.StartDate, i.DueDate, i.ParentIssueId)).ToList(),
            dependencies.Select(d => new GanttDependencyDto(d.Id, d.IssueId, d.DependsOnIssueId, d.DependencyType)).ToList()
        );
    }
}
