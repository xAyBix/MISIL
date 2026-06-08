using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Commands;
public record RemoveTeamMemberCommand(Guid TeamId, Guid UserId) : IRequest;

public class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand>
{
    private readonly ITeamRepository _teamRepo;

    public RemoveTeamMemberCommandHandler(ITeamRepository teamRepo) => _teamRepo = teamRepo;

    public async Task Handle(RemoveTeamMemberCommand request, CancellationToken ct)
    {
        var team = await _teamRepo.GetByIdAsync(request.TeamId, ct);
        if (team == null) throw new NotFoundException(nameof(Team), request.TeamId);
        team.RemoveMember(request.UserId);
        await _teamRepo.UpdateAsync(team, ct);
    }
}
