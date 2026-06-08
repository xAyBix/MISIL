using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Commands;
public record UpdateProjectMemberRoleCommand(Guid ProjectId, Guid UserId, Guid RoleId) : IRequest;

public class UpdateProjectMemberRoleHandler : IRequestHandler<UpdateProjectMemberRoleCommand>
{
    private readonly IProjectRepository _projectRepo;

    public UpdateProjectMemberRoleHandler(IProjectRepository projectRepo) => _projectRepo = projectRepo;

    public async Task Handle(UpdateProjectMemberRoleCommand request, CancellationToken ct)
    {
        var project = await _projectRepo.GetByIdAsync(request.ProjectId, ct);
        if (project == null) throw new NotFoundException(nameof(Project), request.ProjectId);
        await _projectRepo.UpdateMemberRoleAsync(request.ProjectId, request.UserId, request.RoleId, ct);
    }
}
