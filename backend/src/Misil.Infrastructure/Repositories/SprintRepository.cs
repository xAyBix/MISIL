using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;

public class SprintRepository : ISprintRepository
{
    private readonly MisilDbContext _context;

    public SprintRepository(MisilDbContext context) => _context = context;

    public async Task<List<Sprint>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.Sprints
            .AsNoTracking()
            .Include(s => s.SprintIssues)
            .ThenInclude(si => si.Issue)
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Sprint?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Sprints
            .Include(s => s.SprintIssues)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Sprint?> GetActiveSprintAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.Sprints
            .AsNoTracking()
            .Include(s => s.SprintIssues)
            .ThenInclude(si => si.Issue)
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.IsActive && !s.IsCompleted, ct);
    }

    public async Task AddAsync(Sprint sprint, CancellationToken ct = default)
    {
        await _context.Sprints.AddAsync(sprint, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Sprint sprint, CancellationToken ct = default)
    {
        _context.Sprints.Update(sprint);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddIssueAsync(Guid sprintId, Guid issueId, CancellationToken ct = default)
    {
        var sprintIssue = new SprintIssue
        {
            Id = Guid.NewGuid(),
            SprintId = sprintId,
            IssueId = issueId
        };
        await _context.SprintIssues.AddAsync(sprintIssue, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RemoveIssueAsync(Guid sprintId, Guid issueId, CancellationToken ct = default)
    {
        var sprintIssue = await _context.SprintIssues
            .FirstOrDefaultAsync(si => si.SprintId == sprintId && si.IssueId == issueId, ct);
        if (sprintIssue != null)
        {
            _context.SprintIssues.Remove(sprintIssue);
            await _context.SaveChangesAsync(ct);
        }
    }
}
