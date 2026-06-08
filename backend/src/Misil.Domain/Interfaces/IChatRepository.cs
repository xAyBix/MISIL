using Misil.Domain.Entities;

namespace Misil.Domain.Interfaces;

public interface IChatRepository
{
    Task<List<ChatChannel>> GetChannelsAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    Task<List<ChatMessage>> GetMessagesAsync(Guid channelId, int limit = 50, Guid? before = null, CancellationToken ct = default);
    Task<ChatMessage> AddMessageAsync(ChatMessage message, CancellationToken ct = default);
    Task<ChatChannel> CreateChannelAsync(ChatChannel channel, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid userId, Guid messageId, CancellationToken ct = default);
}
