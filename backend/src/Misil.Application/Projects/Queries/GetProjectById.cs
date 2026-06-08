using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Queries;
public record GetProjectByIdQuery(Guid Id, Guid UserId) : IRequest<ProjectDetailDto>;

public record ProjectDetailDto(Guid Id, string Name, string? Description, string Key, Guid LeadUserId, DateTime CreatedAt, List<ProjectMemberDto> Members);

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepo;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepo, IUserRepository userRepo)
    {
        _projectRepo = projectRepo;
        _userRepo = userRepo;
    }

    public async Task<ProjectDetailDto> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await _projectRepo.GetByIdAsync(request.Id, ct);
        if (project == null) throw new NotFoundException(nameof(Project), request.Id);

        if (!project.Members.Any(m => m.UserId == request.UserId))
            throw new UnauthorizedAccessException("Not a member of this project");

        var members = new List<ProjectMemberDto>();
        foreach (var m in project.Members)
        {
            var user = await _userRepo.GetByIdAsync(m.UserId, ct);
            members.Add(new ProjectMemberDto(m.UserId, user?.UserName ?? "", user?.Email ?? "", m.RoleId, user?.DisplayName, user?.AvatarUrl, m.JoinedAt));
        }

        return new ProjectDetailDto(project.Id, project.Name, project.Description, project.Key, project.LeadUserId, project.CreatedAt, members);
    }
}
