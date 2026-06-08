using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly MisilDbContext _context;

    public TeamRepository(MisilDbContext context) => _context = context;

    public async Task<List<Team>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.Teams
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Members)
            .ToListAsync(ct);
    }

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task AddAsync(Team team, CancellationToken ct = default)
    {
        await _context.Teams.AddAsync(team, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Team team, CancellationToken ct = default)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var team = await _context.Teams.FindAsync([id], ct);
        if (team != null)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<List<TeamMember>> GetMembersAsync(Guid teamId, CancellationToken ct = default)
    {
        return await _context.TeamMembers
            .AsNoTracking()
            .Where(tm => tm.TeamId == teamId)
            .ToListAsync(ct);
    }

    public async Task AddMemberAsync(TeamMember member, CancellationToken ct = default)
    {
        await _context.TeamMembers.AddAsync(member, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        var member = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, ct);
        if (member != null)
        {
            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync(ct);
        }
    }
}
