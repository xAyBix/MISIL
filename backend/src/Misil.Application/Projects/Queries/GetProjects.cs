using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Projects.Queries;
public record GetProjectsQuery(Guid UserId) : IRequest<List<ProjectDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IProjectRepository _projectRepo;

    public GetProjectsQueryHandler(IProjectRepository projectRepo) => _projectRepo = projectRepo;

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var projects = await _projectRepo.GetUserProjectsAsync(request.UserId, ct);
        return projects.Select(p => new ProjectDto(
            p.Id, p.Name, p.Description, p.Key, p.LeadUserId, p.Members.Count, p.CreatedAt
        )).ToList();
    }
}
