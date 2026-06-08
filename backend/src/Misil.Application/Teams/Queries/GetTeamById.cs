using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Queries;
public record GetTeamByIdQuery(Guid Id) : IRequest<TeamDto>;

public class GetTeamByIdQueryHandler : IRequestHandler<GetTeamByIdQuery, TeamDto>
{
    private readonly ITeamRepository _teamRepo;

    public GetTeamByIdQueryHandler(ITeamRepository teamRepo) => _teamRepo = teamRepo;

    public async Task<TeamDto> Handle(GetTeamByIdQuery request, CancellationToken ct)
    {
        var team = await _teamRepo.GetByIdAsync(request.Id, ct);
        if (team == null) throw new NotFoundException(nameof(Team), request.Id);
        return new TeamDto(team.Id, team.Name, team.Description, team.ProjectId, team.Members.Count, team.CreatedAt);
    }
}
