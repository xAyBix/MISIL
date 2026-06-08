using Mapster;
using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly MisilDbContext _context;

    public ProjectRepository(MisilDbContext context) => _context = context;

    public async Task<List<Project>> GetUserProjectsAsync(Guid userId, CancellationToken ct = default)
    {
        var projectIds = await _context.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync(ct);

        return await _context.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .Where(p => projectIds.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project> AddAsync(Project project, CancellationToken ct = default)
    {
        await _context.Projects.AddAsync(project, ct);
        await _context.SaveChangesAsync(ct);
        return project;
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _context.Projects.FindAsync([id], ct);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<List<ProjectMember>> GetMembersAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync(ct);
    }

    public async Task AddMemberAsync(ProjectMember member, CancellationToken ct = default)
    {
        await _context.ProjectMembers.AddAsync(member, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateMemberRoleAsync(Guid projectId, Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, ct);
        if (member != null)
        {
            member.RoleId = roleId;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, ct);
        if (member != null)
        {
            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync(ct);
        }
    }
}
