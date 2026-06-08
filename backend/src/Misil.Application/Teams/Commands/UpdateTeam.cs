using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Commands;
public record UpdateTeamCommand(Guid Id, string Name, string? Description) : IRequest<TeamDto>;

public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, TeamDto>
{
    private readonly ITeamRepository _teamRepo;

    public UpdateTeamCommandHandler(ITeamRepository teamRepo) => _teamRepo = teamRepo;

    public async Task<TeamDto> Handle(UpdateTeamCommand request, CancellationToken ct)
    {
        var team = await _teamRepo.GetByIdAsync(request.Id, ct);
        if (team == null) throw new NotFoundException(nameof(Team), request.Id);
        team.Update(request.Name, request.Description);
        await _teamRepo.UpdateAsync(team, ct);
        return new TeamDto(team.Id, team.Name, team.Description, team.ProjectId, team.Members.Count, team.CreatedAt);
    }
}
