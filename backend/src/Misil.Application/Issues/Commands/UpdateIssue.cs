using Mapster;
using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Commands;
public record UpdateIssueCommand(
    Guid Id, string? Title, string? Description, IssueType? Type, IssuePriority? Priority,
    int? StoryPoints, DateTime? StartDate, DateTime? DueDate) : IRequest<IssueDto>;

public class UpdateIssueCommandHandler : IRequestHandler<UpdateIssueCommand, IssueDto>
{
    private readonly IIssueRepository _issueRepo;

    public UpdateIssueCommandHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task<IssueDto> Handle(UpdateIssueCommand request, CancellationToken ct)
    {
        var issue = await _issueRepo.GetByIdAsync(request.Id, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.Id);

        if (request.Title != null) issue.Title = request.Title;
        if (request.Description != null) issue.Description = request.Description;
        if (request.Type.HasValue) issue.IssueType = request.Type.Value;
        if (request.Priority.HasValue) issue.Priority = request.Priority.Value;
        if (request.StoryPoints.HasValue) issue.StoryPoints = request.StoryPoints;
        if (request.StartDate.HasValue) issue.StartDate = DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc);
        if (request.DueDate.HasValue) issue.DueDate = DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc);
        issue.UpdatedAt = DateTime.UtcNow;

        await _issueRepo.UpdateAsync(issue, ct);
        return issue.Adapt<IssueDto>();
    }
}
