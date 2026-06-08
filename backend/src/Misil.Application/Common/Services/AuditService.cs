using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Common.Services;
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repo;
    public AuditService(IAuditLogRepository repo) => _repo = repo;

    public async Task LogAsync(Guid projectId, Guid userId, string userName, EntityType entityType, string entityId, string action, string? oldValue = null, string? newValue = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            UserName = userName,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(log, ct);
    }
}
