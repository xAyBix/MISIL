using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Commands;
public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest;

public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand>
{
    private readonly IProjectRepository _projectRepo;

    public RemoveProjectMemberCommandHandler(IProjectRepository projectRepo) => _projectRepo = projectRepo;

    public async Task Handle(RemoveProjectMemberCommand request, CancellationToken ct)
    {
        var project = await _projectRepo.GetByIdAsync(request.ProjectId, ct);
        if (project == null) throw new NotFoundException(nameof(Project), request.ProjectId);

        await _projectRepo.RemoveMemberAsync(request.ProjectId, request.UserId, ct);
    }
}
