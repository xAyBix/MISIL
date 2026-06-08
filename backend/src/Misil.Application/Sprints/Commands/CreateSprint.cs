using Mapster;
using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Commands;
public record CreateSprintCommand(string Name, string? Goal, Guid ProjectId, DateTime? StartDate, DateTime? EndDate) : IRequest<SprintDto>;

public class CreateSprintCommandHandler : IRequestHandler<CreateSprintCommand, SprintDto>
{
    private readonly ISprintRepository _sprintRepo;

    public CreateSprintCommandHandler(ISprintRepository sprintRepo) => _sprintRepo = sprintRepo;

    public async Task<SprintDto> Handle(CreateSprintCommand request, CancellationToken ct)
    {
        DateTime? start = request.StartDate is DateTime sd ? DateTime.SpecifyKind(sd, DateTimeKind.Utc) : null;
        DateTime? end = request.EndDate is DateTime ed ? DateTime.SpecifyKind(ed, DateTimeKind.Utc) : null;
        var sprint = new Sprint(request.Name, request.Goal, request.ProjectId, start, end);
        await _sprintRepo.AddAsync(sprint, ct);
        return sprint.Adapt<SprintDto>();
    }
}
