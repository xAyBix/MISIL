using Mapster;
using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Commands;
public record CreateIssueCommand(
    string Title, string? Description, IssueType Type, IssuePriority Priority,
    Guid ProjectId, Guid? SprintId, Guid? AssigneeId, Guid? AssigneeTeamId, Guid ReporterId, Guid? ParentId,
    int? StoryPoints, DateTime? StartDate, DateTime? DueDate) : IRequest<IssueDto>;

public class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, IssueDto>
{
    private readonly IIssueRepository _issueRepo;

    public CreateIssueCommandHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task<IssueDto> Handle(CreateIssueCommand request, CancellationToken ct)
    {
        if (request.AssigneeId.HasValue && request.AssigneeTeamId.HasValue)
            throw new InvalidOperationException("Issue cannot be assigned to both a member and a team");

        var issue = new Issue(request.ProjectId, request.Type, request.Title, request.ReporterId, request.Description)
        {
            Priority = request.Priority,
            AssigneeId = request.AssigneeId,
            AssigneeTeamId = request.AssigneeTeamId,
            ParentIssueId = request.ParentId,
            StoryPoints = request.StoryPoints,
            StartDate = request.StartDate is DateTime sd ? DateTime.SpecifyKind(sd, DateTimeKind.Utc) : null,
            DueDate = request.DueDate is DateTime dd ? DateTime.SpecifyKind(dd, DateTimeKind.Utc) : null
        };

        await _issueRepo.AddAsync(issue, ct);
        return issue.Adapt<IssueDto>();
    }
}
