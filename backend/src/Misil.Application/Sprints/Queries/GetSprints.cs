using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Queries;
public record GetSprintsQuery(Guid ProjectId) : IRequest<List<SprintDto>>;

public class GetSprintsQueryHandler : IRequestHandler<GetSprintsQuery, List<SprintDto>>
{
    private readonly ISprintRepository _sprintRepo;

    public GetSprintsQueryHandler(ISprintRepository sprintRepo) => _sprintRepo = sprintRepo;

    public async Task<List<SprintDto>> Handle(GetSprintsQuery request, CancellationToken ct)
    {
        var sprints = await _sprintRepo.GetByProjectIdAsync(request.ProjectId, ct);
        return sprints.Select(SprintDto.FromSprint).ToList();
    }
}
