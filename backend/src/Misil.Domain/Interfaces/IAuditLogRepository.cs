using Misil.Domain.Entities;
using Misil.Domain.Enums;

namespace Misil.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetFilteredAsync(Guid? projectId = null, EntityType? entityType = null, Guid? userId = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task AddAsync(AuditLog auditLog, CancellationToken ct = default);
}
