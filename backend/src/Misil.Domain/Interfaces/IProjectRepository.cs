using Misil.Domain.Entities;

namespace Misil.Domain.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetUserProjectsAsync(Guid userId, CancellationToken ct = default);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Project> AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<ProjectMember>> GetMembersAsync(Guid projectId, CancellationToken ct = default);
    Task AddMemberAsync(ProjectMember member, CancellationToken ct = default);
    Task UpdateMemberRoleAsync(Guid projectId, Guid userId, Guid roleId, CancellationToken ct = default);
    Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken ct = default);
}
