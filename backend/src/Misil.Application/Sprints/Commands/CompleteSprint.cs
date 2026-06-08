using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Sprints.Commands;
public record CompleteSprintCommand(Guid Id) : IRequest;

public class CompleteSprintCommandHandler : IRequestHandler<CompleteSprintCommand>
{
    private readonly ISprintRepository _sprintRepo;

    public CompleteSprintCommandHandler(ISprintRepository sprintRepo) => _sprintRepo = sprintRepo;

    public async Task Handle(CompleteSprintCommand request, CancellationToken ct)
    {
        var sprint = await _sprintRepo.GetByIdAsync(request.Id, ct);
        if (sprint == null) throw new NotFoundException(nameof(Sprint), request.Id);

        sprint.IsActive = false;
        sprint.IsCompleted = true;

        await _sprintRepo.UpdateAsync(sprint, ct);
    }
}
