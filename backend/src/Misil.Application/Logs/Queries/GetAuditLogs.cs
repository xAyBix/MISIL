using MediatR;
using Misil.Application.Common.DTOs;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Logs.Queries;
public record GetAuditLogsQuery(Guid ProjectId, EntityType? EntityType, Guid? UserId, DateTime? From, DateTime? To, int Page = 1, int PageSize = 20) : IRequest<PagedResult<AuditLogEntryDto>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogEntryDto>>
{
    private readonly IAuditLogRepository _auditLogRepo;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepo) => _auditLogRepo = auditLogRepo;

    public async Task<PagedResult<AuditLogEntryDto>> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        var logs = await _auditLogRepo.GetFilteredAsync(request.ProjectId, request.EntityType,
            request.UserId, request.From, request.To, request.Page, request.PageSize, ct);

        var items = logs.Select(l => new AuditLogEntryDto(l.Id, l.EntityType, l.EntityId, l.UserId, l.UserName, l.Action, l.OldValue, l.NewValue, l.CreatedAt)).ToList();
        return new PagedResult<AuditLogEntryDto>(items, logs.Count, request.Page, request.PageSize);
    }
}
