using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Commands;
public record DeleteIssueCommand(Guid Id) : IRequest;

public class DeleteIssueCommandHandler : IRequestHandler<DeleteIssueCommand>
{
    private readonly IIssueRepository _issueRepo;

    public DeleteIssueCommandHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task Handle(DeleteIssueCommand request, CancellationToken ct)
    {
        var issue = await _issueRepo.GetByIdAsync(request.Id, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.Id);
        await _issueRepo.DeleteAsync(request.Id, ct);
    }
}
