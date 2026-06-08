using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Issues.Commands;
public record AddCommentCommand(Guid IssueId, Guid UserId, string Content) : IRequest<CommentDto>;

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, CommentDto>
{
    private readonly IIssueRepository _issueRepo;

    public AddCommentCommandHandler(IIssueRepository issueRepo) => _issueRepo = issueRepo;

    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var issue = await _issueRepo.GetByIdAsync(request.IssueId, ct);
        if (issue == null) throw new NotFoundException(nameof(Issue), request.IssueId);

        var comment = new IssueComment
        {
            Id = Guid.NewGuid(),
            IssueId = request.IssueId,
            UserId = request.UserId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        comment = await _issueRepo.AddCommentAsync(comment, ct);
        return new CommentDto(comment.Id, comment.IssueId, comment.UserId, comment.Content, comment.CreatedAt);
    }
}
