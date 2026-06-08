using Mapster;
using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Commands;
public record AssignIssueCommand(Guid Id, Guid? AssigneeId, Guid? AssigneeTeamId) : IRequest<IssueDto>;

public class AssignIssueCommandHandler : IRequestHandler<AssignIssueCommand, IssueDto>
{
    private readonly IIssueRepository _issueRepo;

    public AssignIssueCommandHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task<IssueDto> Handle(AssignIssueCommand request, CancellationToken ct)
    {
        if (request.AssigneeId.HasValue && request.AssigneeTeamId.HasValue)
            throw new InvalidOperationException("Issue cannot be assigned to both a member and a team");

        var issue = await _issueRepo.GetByIdAsync(request.Id, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.Id);

        issue.AssigneeId = request.AssigneeId;
        issue.AssigneeTeamId = request.AssigneeTeamId;
        issue.UpdatedAt = DateTime.UtcNow;

        await _issueRepo.UpdateAsync(issue, ct);
        return issue.Adapt<IssueDto>();
    }
}
