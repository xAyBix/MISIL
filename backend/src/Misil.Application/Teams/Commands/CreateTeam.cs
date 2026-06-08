using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Commands;
public record CreateTeamCommand(string Name, string? Description, Guid ProjectId) : IRequest<TeamDto>;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, TeamDto>
{
    private readonly ITeamRepository _teamRepo;

    public CreateTeamCommandHandler(ITeamRepository teamRepo) => _teamRepo = teamRepo;

    public async Task<TeamDto> Handle(CreateTeamCommand request, CancellationToken ct)
    {
        var team = new Team(request.Name, request.Description, request.ProjectId);
        await _teamRepo.AddAsync(team, ct);
        return new TeamDto(team.Id, team.Name, team.Description, team.ProjectId, team.Members.Count, team.CreatedAt);
    }
}
