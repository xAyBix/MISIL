using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Commands;
public record UpdateIssueStatusCommand(Guid Id, IssueStatus Status) : IRequest;

public class UpdateIssueStatusCommandHandler : IRequestHandler<UpdateIssueStatusCommand>
{
    private readonly IIssueRepository _issueRepo;

    public UpdateIssueStatusCommandHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task Handle(UpdateIssueStatusCommand request, CancellationToken ct)
    {
        var issue = await _issueRepo.GetByIdAsync(request.Id, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.Id);
        issue.Status = request.Status;
        issue.UpdatedAt = DateTime.UtcNow;
        await _issueRepo.UpdateAsync(issue, ct);
    }
}
