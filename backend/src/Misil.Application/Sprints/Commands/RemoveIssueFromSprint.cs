using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Commands;
public record RemoveIssueFromSprintCommand(Guid SprintId, Guid IssueId) : IRequest;

public class RemoveIssueFromSprintCommandHandler : IRequestHandler<RemoveIssueFromSprintCommand>
{
    private readonly ISprintRepository _sprintRepo;
    private readonly IIssueRepository _issueRepo;

    public RemoveIssueFromSprintCommandHandler(ISprintRepository sprintRepo, IIssueRepository issueRepo)
    {
        _sprintRepo = sprintRepo;
        _issueRepo = issueRepo;
    }

    public async Task Handle(RemoveIssueFromSprintCommand request, CancellationToken ct)
    {
        var sprint = await _sprintRepo.GetByIdAsync(request.SprintId, ct);
        if (sprint == null) throw new NotFoundException(nameof(Sprint), request.SprintId);

        var issue = await _issueRepo.GetByIdAsync(request.IssueId, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.IssueId);

        await _sprintRepo.RemoveIssueAsync(request.SprintId, request.IssueId, ct);
    }
}
