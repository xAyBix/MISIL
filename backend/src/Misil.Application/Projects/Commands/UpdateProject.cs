using Mapster;
using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Commands;
public record UpdateProjectCommand(Guid Id, string Name, string? Description) : IRequest<ProjectDto>;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepo;

    public UpdateProjectCommandHandler(IProjectRepository projectRepo) => _projectRepo = projectRepo;

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken ct)
    {
        var project = await _projectRepo.GetByIdAsync(request.Id, ct);
        if (project == null) throw new NotFoundException(nameof(Project), request.Id);

        project.Name = request.Name;
        project.Description = request.Description;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project, ct);
        return project.Adapt<ProjectDto>();
    }
}
