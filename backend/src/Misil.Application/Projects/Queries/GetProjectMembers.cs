using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Queries;
public record GetProjectMembersQuery(Guid ProjectId) : IRequest<List<ProjectMemberDto>>;

public record ProjectMemberDto(Guid UserId, string UserName, string Email, Guid RoleId, string? DisplayName, string? AvatarUrl, DateTime JoinedAt);

public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberDto>>
{
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepo;

    public GetProjectMembersQueryHandler(IProjectRepository projectRepo, IUserRepository userRepo)
    {
        _projectRepo = projectRepo;
        _userRepo = userRepo;
    }

    public async Task<List<ProjectMemberDto>> Handle(GetProjectMembersQuery request, CancellationToken ct)
    {
        var project = await _projectRepo.GetByIdAsync(request.ProjectId, ct);
        if (project == null) throw new NotFoundException(nameof(Project), request.ProjectId);

        var members = new List<ProjectMemberDto>();
        foreach (var m in project.Members)
        {
            var user = await _userRepo.GetByIdAsync(m.UserId, ct);
            members.Add(new ProjectMemberDto(m.UserId, user?.UserName ?? "", user?.Email ?? "", m.RoleId, user?.DisplayName, user?.AvatarUrl, m.JoinedAt));
        }

        return members;
    }
}
