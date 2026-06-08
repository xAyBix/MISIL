using Misil.Domain.Enums;

namespace Misil.Application.Common.DTOs;
public record AuditLogEntryDto(Guid Id, EntityType EntityType, string EntityId, Guid UserId, string UserName, string Action, string? OldValue, string? NewValue, DateTime CreatedAt);
