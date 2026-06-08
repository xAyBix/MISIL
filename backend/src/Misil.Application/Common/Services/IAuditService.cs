using Misil.Domain.Enums;

namespace Misil.Application.Common.Services;
public interface IAuditService
{
    Task LogAsync(Guid projectId, Guid userId, string userName, EntityType entityType, string entityId, string action, string? oldValue = null, string? newValue = null, CancellationToken ct = default);
}
