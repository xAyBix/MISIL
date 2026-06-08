using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly MisilDbContext _context;

    public AuditLogRepository(MisilDbContext context) => _context = context;

    public async Task<List<AuditLog>> GetFilteredAsync(Guid? projectId = null, EntityType? entityType = null, Guid? userId = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (projectId.HasValue)
            query = query.Where(al => al.ProjectId == projectId.Value);
        if (entityType.HasValue)
            query = query.Where(al => al.EntityType == entityType.Value);
        if (userId.HasValue)
            query = query.Where(al => al.UserId == userId.Value);
        if (from.HasValue)
            query = query.Where(al => al.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(al => al.CreatedAt <= to.Value);

        return await query
            .OrderByDescending(al => al.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken ct = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, ct);
        await _context.SaveChangesAsync(ct);
    }
}
