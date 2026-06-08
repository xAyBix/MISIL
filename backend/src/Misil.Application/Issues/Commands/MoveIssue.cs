using Mapster;
using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Commands;
public record MoveIssueCommand(Guid Id, IssueStatus NewStatus, int NewOrder) : IRequest<IssueDto>;

public class MoveIssueCommandHandler : IRequestHandler<MoveIssueCommand, IssueDto>
{
    private readonly IIssueRepository _issueRepo;

    public MoveIssueCommandHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task<IssueDto> Handle(MoveIssueCommand request, CancellationToken ct)
    {
        var issue = await _issueRepo.GetByIdAsync(request.Id, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.Id);

        issue.Status = request.NewStatus;
        issue.Order = request.NewOrder;
        issue.UpdatedAt = DateTime.UtcNow;

        await _issueRepo.UpdateAsync(issue, ct);
        return issue.Adapt<IssueDto>();
    }
}
