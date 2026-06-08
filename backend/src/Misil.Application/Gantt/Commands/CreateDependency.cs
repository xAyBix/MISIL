using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Enums;
using Misil.Domain.Interfaces;

namespace Misil.Application.Gantt.Commands;
public record CreateDependencyCommand(Guid IssueId, Guid DependsOnIssueId, DependencyType DependencyType) : IRequest<GanttDependencyDto>;

public class CreateDependencyCommandHandler : IRequestHandler<CreateDependencyCommand, GanttDependencyDto>
{
    private readonly IGanttRepository _ganttRepo;

    public CreateDependencyCommandHandler(IGanttRepository ganttRepo) => _ganttRepo = ganttRepo;

    public async Task<GanttDependencyDto> Handle(CreateDependencyCommand request, CancellationToken ct)
    {
        var dependency = new IssueDependency(request.IssueId, request.DependsOnIssueId, request.DependencyType);
        await _ganttRepo.AddDependencyAsync(dependency, ct);
        return new GanttDependencyDto(dependency.Id, dependency.IssueId, dependency.DependsOnIssueId, dependency.DependencyType);
    }
}
