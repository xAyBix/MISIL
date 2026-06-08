namespace Misil.Application.Chat;
public record ChannelDto(Guid Id, Guid ProjectId, string Name, Guid? TeamId, bool IsPrivate, DateTime CreatedAt);
public record MessageDto(Guid Id, Guid ChannelId, Guid UserId, string Content, DateTime CreatedAt, bool IsRead);
