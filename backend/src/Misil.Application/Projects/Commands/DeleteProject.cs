using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Commands;
public record DeleteProjectCommand(Guid Id) : IRequest;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _projectRepo;

    public DeleteProjectCommandHandler(IProjectRepository projectRepo) => _projectRepo = projectRepo;

    public async Task Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var project = await _projectRepo.GetByIdAsync(request.Id, ct);
        if (project == null) throw new NotFoundException(nameof(Project), request.Id);
        await _projectRepo.DeleteAsync(request.Id, ct);
    }
}
