using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;

public class GanttRepository : IGanttRepository
{
    private readonly MisilDbContext _context;

    public GanttRepository(MisilDbContext context) => _context = context;

    public async Task<List<Issue>> GetGanttIssuesAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.Issues
            .AsNoTracking()
            .Where(i => i.ProjectId == projectId && (i.StartDate != null || i.DueDate != null))
            .OrderBy(i => i.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<IssueDependency>> GetDependenciesAsync(Guid projectId, CancellationToken ct = default)
    {
        var issueIds = await _context.Issues
            .AsNoTracking()
            .Where(i => i.ProjectId == projectId)
            .Select(i => i.Id)
            .ToListAsync(ct);

        return await _context.IssueDependencies
            .AsNoTracking()
            .Where(d => issueIds.Contains(d.IssueId))
            .ToListAsync(ct);
    }

    public async Task AddDependencyAsync(IssueDependency dependency, CancellationToken ct = default)
    {
        await _context.IssueDependencies.AddAsync(dependency, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteDependencyAsync(Guid id, CancellationToken ct = default)
    {
        var dep = await _context.IssueDependencies.FindAsync([id], ct);
        if (dep != null)
        {
            _context.IssueDependencies.Remove(dep);
            await _context.SaveChangesAsync(ct);
        }
    }
}
