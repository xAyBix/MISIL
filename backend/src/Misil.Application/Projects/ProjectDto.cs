namespace Misil.Application.Projects;
public record ProjectDto(Guid Id, string Name, string? Description, string Key, Guid LeadUserId, int MemberCount, DateTime CreatedAt);
