using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Commands;
public record DeleteTeamCommand(Guid Id) : IRequest;

public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand>
{
    private readonly ITeamRepository _teamRepo;

    public DeleteTeamCommandHandler(ITeamRepository teamRepo) => _teamRepo = teamRepo;

    public async Task Handle(DeleteTeamCommand request, CancellationToken ct)
    {
        var team = await _teamRepo.GetByIdAsync(request.Id, ct);
        if (team == null) throw new NotFoundException(nameof(Team), request.Id);
        await _teamRepo.DeleteAsync(request.Id, ct);
    }
}
