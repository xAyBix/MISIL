using Misil.Domain.Entities;
using Misil.Domain.Enums;

namespace Misil.Domain.Interfaces;

public interface IIssueRepository
{
    Task<List<Issue>> GetFilteredAsync(Guid? projectId = null, Guid? assigneeId = null, Guid? assigneeTeamId = null, Guid? sprintId = null, IssueStatus? status = null, IssueType? issueType = null, CancellationToken ct = default);
    Task<Issue?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Issue> AddAsync(Issue issue, CancellationToken ct = default);
    Task UpdateAsync(Issue issue, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IssueComment> AddCommentAsync(IssueComment comment, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid issueId, IssueStatus status, CancellationToken ct = default);
}
