using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Commands;
public record StartSprintCommand(Guid Id) : IRequest;

public class StartSprintCommandHandler : IRequestHandler<StartSprintCommand>
{
    private readonly ISprintRepository _sprintRepo;

    public StartSprintCommandHandler(ISprintRepository sprintRepo) => _sprintRepo = sprintRepo;

    public async Task Handle(StartSprintCommand request, CancellationToken ct)
    {
        var sprint = await _sprintRepo.GetByIdAsync(request.Id, ct);
        if (sprint == null) throw new NotFoundException(nameof(Sprint), request.Id);

        sprint.IsActive = true;
        sprint.StartDate ??= DateTime.UtcNow;

        await _sprintRepo.UpdateAsync(sprint, ct);
    }
}
