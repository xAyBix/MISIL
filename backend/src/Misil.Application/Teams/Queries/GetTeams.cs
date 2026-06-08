using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Queries;
public record GetTeamsQuery(Guid ProjectId) : IRequest<List<TeamDto>>;

public class GetTeamsQueryHandler : IRequestHandler<GetTeamsQuery, List<TeamDto>>
{
    private readonly ITeamRepository _teamRepo;

    public GetTeamsQueryHandler(ITeamRepository teamRepo) => _teamRepo = teamRepo;

    public async Task<List<TeamDto>> Handle(GetTeamsQuery request, CancellationToken ct)
    {
        var teams = await _teamRepo.GetByProjectIdAsync(request.ProjectId, ct);
        return teams.Select(t => new TeamDto(t.Id, t.Name, t.Description, t.ProjectId, t.Members.Count, t.CreatedAt)).ToList();
    }
}
