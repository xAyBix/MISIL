using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Commands;
public record AddProjectMemberCommand(Guid ProjectId, Guid UserId, Guid RoleId) : IRequest;

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand>
{
    private readonly IProjectRepository _projectRepo;

    public AddProjectMemberCommandHandler(IProjectRepository projectRepo) => _projectRepo = projectRepo;

    public async Task Handle(AddProjectMemberCommand request, CancellationToken ct)
    {
        var project = await _projectRepo.GetByIdAsync(request.ProjectId, ct);
        if (project == null) throw new NotFoundException(nameof(Project), request.ProjectId);

        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            RoleId = request.RoleId,
            JoinedAt = DateTime.UtcNow
        };

        await _projectRepo.AddMemberAsync(member, ct);
    }
}
