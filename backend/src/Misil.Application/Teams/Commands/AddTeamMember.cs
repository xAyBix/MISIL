using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Commands;
public record AddTeamMemberCommand(Guid TeamId, Guid UserId) : IRequest;

public class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand>
{
    private readonly ITeamRepository _teamRepo;

    public AddTeamMemberCommandHandler(ITeamRepository teamRepo) => _teamRepo = teamRepo;

    public async Task Handle(AddTeamMemberCommand request, CancellationToken ct)
    {
        var team = await _teamRepo.GetByIdAsync(request.TeamId, ct);
        if (team == null) throw new NotFoundException(nameof(Team), request.TeamId);
        team.AddMember(request.UserId);
        await _teamRepo.UpdateAsync(team, ct);
    }
}
