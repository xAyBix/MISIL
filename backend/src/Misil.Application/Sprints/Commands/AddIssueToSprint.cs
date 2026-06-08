using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Commands;
public record AddIssueToSprintCommand(Guid SprintId, Guid IssueId) : IRequest;

public class AddIssueToSprintCommandHandler : IRequestHandler<AddIssueToSprintCommand>
{
    private readonly ISprintRepository _sprintRepo;
    private readonly IIssueRepository _issueRepo;

    public AddIssueToSprintCommandHandler(ISprintRepository sprintRepo, IIssueRepository issueRepo)
    {
        _sprintRepo = sprintRepo;
        _issueRepo = issueRepo;
    }

    public async Task Handle(AddIssueToSprintCommand request, CancellationToken ct)
    {
        var sprint = await _sprintRepo.GetByIdAsync(request.SprintId, ct);
        if (sprint == null) throw new NotFoundException(nameof(Sprint), request.SprintId);

        var issue = await _issueRepo.GetByIdAsync(request.IssueId, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.IssueId);

        await _sprintRepo.AddIssueAsync(request.SprintId, request.IssueId, ct);
    }
}
