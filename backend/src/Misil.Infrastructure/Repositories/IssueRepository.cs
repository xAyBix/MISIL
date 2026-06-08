using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly MisilDbContext _context;

    public IssueRepository(MisilDbContext context) => _context = context;

    public async Task<List<Issue>> GetFilteredAsync(Guid? projectId = null, Guid? assigneeId = null, Guid? assigneeTeamId = null, Guid? sprintId = null, IssueStatus? status = null, IssueType? issueType = null, CancellationToken ct = default)
    {
        var query = _context.Issues.AsNoTracking();

        if (projectId.HasValue)
            query = query.Where(i => i.ProjectId == projectId.Value);
        if (assigneeId.HasValue)
            query = query.Where(i => i.AssigneeId == assigneeId.Value);
        if (assigneeTeamId.HasValue)
            query = query.Where(i => i.AssigneeTeamId == assigneeTeamId.Value);
        if (sprintId.HasValue)
            query = query.Where(i => i.SprintIssues.Any(si => si.SprintId == sprintId.Value));
        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);
        if (issueType.HasValue)
            query = query.Where(i => i.IssueType == issueType.Value);

        return await query.OrderBy(i => i.Order).ToListAsync(ct);
    }

    public async Task<Issue?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Issues
            .Include(i => i.Comments)
            .Include(i => i.Attachments)
            .Include(i => i.RelatedIssues)
            .Include(i => i.Dependencies)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Issue> AddAsync(Issue issue, CancellationToken ct = default)
    {
        await _context.Issues.AddAsync(issue, ct);
        await _context.SaveChangesAsync(ct);
        return issue;
    }

    public async Task UpdateAsync(Issue issue, CancellationToken ct = default)
    {
        _context.Issues.Update(issue);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var issue = await _context.Issues.FindAsync([id], ct);
        if (issue != null)
        {
            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<IssueComment> AddCommentAsync(IssueComment comment, CancellationToken ct = default)
    {
        await _context.IssueComments.AddAsync(comment, ct);
        await _context.SaveChangesAsync(ct);
        return comment;
    }

    public async Task UpdateStatusAsync(Guid issueId, IssueStatus status, CancellationToken ct = default)
    {
        var issue = await _context.Issues.FindAsync([issueId], ct);
        if (issue != null)
        {
            issue.Status = status;
            await _context.SaveChangesAsync(ct);
        }
    }
}
