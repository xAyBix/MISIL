using Mapster;
using MediatR;
using Misil.Application.Common.DTOs;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Queries;
public record GetIssuesQuery(Guid ProjectId, IssueStatus? Status, Guid? SprintId, Guid? AssigneeId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<IssueDto>>;

public class GetIssuesQueryHandler : IRequestHandler<GetIssuesQuery, PagedResult<IssueDto>>
{
    private readonly IIssueRepository _issueRepo;

    public GetIssuesQueryHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task<PagedResult<IssueDto>> Handle(GetIssuesQuery request, CancellationToken ct)
    {
        var issues = await _issueRepo.GetFilteredAsync(request.ProjectId, request.AssigneeId, null, request.SprintId, request.Status, null, ct);
        var total = issues.Count;
        var paged = issues.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
        return new PagedResult<IssueDto>(paged.Adapt<List<IssueDto>>(), total, request.Page, request.PageSize);
    }
}
