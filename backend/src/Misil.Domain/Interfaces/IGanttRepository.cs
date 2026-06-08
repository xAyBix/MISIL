using Misil.Domain.Entities;

namespace Misil.Domain.Interfaces;

public interface IGanttRepository
{
    Task<List<Issue>> GetGanttIssuesAsync(Guid projectId, CancellationToken ct = default);
    Task<List<IssueDependency>> GetDependenciesAsync(Guid projectId, CancellationToken ct = default);
    Task AddDependencyAsync(IssueDependency dependency, CancellationToken ct = default);
    Task DeleteDependencyAsync(Guid id, CancellationToken ct = default);
}
