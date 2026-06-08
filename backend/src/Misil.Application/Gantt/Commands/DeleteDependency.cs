using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Gantt.Commands;
public record DeleteDependencyCommand(Guid Id) : IRequest;

public class DeleteDependencyCommandHandler : IRequestHandler<DeleteDependencyCommand>
{
    private readonly IGanttRepository _ganttRepo;

    public DeleteDependencyCommandHandler(IGanttRepository ganttRepo) => _ganttRepo = ganttRepo;

    public async Task Handle(DeleteDependencyCommand request, CancellationToken ct)
    {
        await _ganttRepo.DeleteDependencyAsync(request.Id, ct);
    }
}
