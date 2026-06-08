using Mapster;
using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Queries;
public record GetIssueByIdQuery(Guid Id) : IRequest<IssueDetailDto>;

public class GetIssueByIdQueryHandler : IRequestHandler<GetIssueByIdQuery, IssueDetailDto>
{
    private readonly IIssueRepository _issueRepo;

    public GetIssueByIdQueryHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task<IssueDetailDto> Handle(GetIssueByIdQuery request, CancellationToken ct)
    {
        var issue = await _issueRepo.GetByIdAsync(request.Id, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.Id);

        var comments = issue.Comments.Select(c => new CommentDto(c.Id, c.IssueId, c.UserId, c.Content, c.CreatedAt)).ToList();
        var attachments = issue.Attachments.Select(a => new AttachmentDto(a.Id, a.IssueId, a.FileName, a.FileUrl, a.UserId, a.UploadedAt)).ToList();

        return new IssueDetailDto(issue.Id, issue.Title, issue.Description, issue.IssueType, issue.Status, issue.Priority,
            issue.ProjectId, issue.AssigneeId, issue.AssigneeTeamId, issue.ReporterId, issue.ParentIssueId, issue.Order,
            issue.StoryPoints, issue.StartDate, issue.DueDate, issue.CreatedAt, issue.UpdatedAt,
            comments, attachments);
    }
}
