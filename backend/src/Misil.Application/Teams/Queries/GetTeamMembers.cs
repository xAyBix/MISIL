using MediatR;
using Misil.Application.Common.Exceptions;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Teams.Queries;
public record GetTeamMembersQuery(Guid TeamId) : IRequest<List<TeamMemberDetailDto>>;

public record TeamMemberDetailDto(Guid UserId, string UserName, string Email, string? DisplayName, string? AvatarUrl, DateTime JoinedAt);

public class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, List<TeamMemberDetailDto>>
{
    private readonly ITeamRepository _teamRepo;
    private readonly IUserRepository _userRepo;

    public GetTeamMembersQueryHandler(ITeamRepository teamRepo, IUserRepository userRepo)
    {
        _teamRepo = teamRepo;
        _userRepo = userRepo;
    }

    public async Task<List<TeamMemberDetailDto>> Handle(GetTeamMembersQuery request, CancellationToken ct)
    {
        var team = await _teamRepo.GetByIdAsync(request.TeamId, ct);
        if (team == null) throw new NotFoundException(nameof(Team), request.TeamId);

        var members = new List<TeamMemberDetailDto>();
        foreach (var tm in team.Members)
        {
            var user = await _userRepo.GetByIdAsync(tm.UserId, ct);
            members.Add(new TeamMemberDetailDto(tm.UserId, user?.UserName ?? "", user?.Email ?? "", user?.DisplayName, user?.AvatarUrl, tm.JoinedAt));
        }
        return members;
    }
}
