using Mapster;
using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Commands;
public record CreateProjectCommand(string Name, string? Description, string Key, Guid LeadUserId) : IRequest<ProjectDto>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IRoleRepository _roleRepo;

    public CreateProjectCommandHandler(IProjectRepository projectRepo, IRoleRepository roleRepo)
    {
        _projectRepo = projectRepo;
        _roleRepo = roleRepo;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        var project = new Project(request.Name, request.Description, request.Key, request.LeadUserId);
        var ownerRole = await _roleRepo.GetByNameAsync("Owner", ct);
        project.AddMember(request.LeadUserId, ownerRole?.Id);
        await _projectRepo.AddAsync(project, ct);
        return project.Adapt<ProjectDto>();
    }
}
