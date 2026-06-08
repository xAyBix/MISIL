namespace Misil.Application.Teams;
public record TeamDto(Guid Id, string Name, string? Description, Guid ProjectId, int MemberCount, DateTime CreatedAt);
public record TeamMemberDto(Guid TeamId, Guid UserId);
