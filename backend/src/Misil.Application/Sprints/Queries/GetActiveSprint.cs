using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Queries;
public record GetActiveSprintQuery(Guid ProjectId) : IRequest<SprintDto?>;

public class GetActiveSprintQueryHandler : IRequestHandler<GetActiveSprintQuery, SprintDto?>
{
    private readonly ISprintRepository _sprintRepo;

    public GetActiveSprintQueryHandler(ISprintRepository sprintRepo) => _sprintRepo = sprintRepo;

    public async Task<SprintDto?> Handle(GetActiveSprintQuery request, CancellationToken ct)
    {
        var sprint = await _sprintRepo.GetActiveSprintAsync(request.ProjectId, ct);
        if (sprint == null) return null;
        return SprintDto.FromSprint(sprint);
    }
}
