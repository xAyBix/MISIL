using Misil.Domain.Entities;

namespace Misil.Domain.Interfaces;

public interface ISprintRepository
{
    Task<List<Sprint>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<Sprint?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Sprint?> GetActiveSprintAsync(Guid projectId, CancellationToken ct = default);
    Task AddAsync(Sprint sprint, CancellationToken ct = default);
    Task UpdateAsync(Sprint sprint, CancellationToken ct = default);
    Task AddIssueAsync(Guid sprintId, Guid issueId, CancellationToken ct = default);
    Task RemoveIssueAsync(Guid sprintId, Guid issueId, CancellationToken ct = default);
}
