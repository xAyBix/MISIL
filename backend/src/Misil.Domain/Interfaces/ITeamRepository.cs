using Misil.Domain.Entities;

namespace Misil.Domain.Interfaces;

public interface ITeamRepository
{
    Task<List<Team>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Team team, CancellationToken ct = default);
    Task UpdateAsync(Team team, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<TeamMember>> GetMembersAsync(Guid teamId, CancellationToken ct = default);
    Task AddMemberAsync(TeamMember member, CancellationToken ct = default);
    Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default);
}
