using Microsoft.EntityFrameworkCore;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;
using Misil.Infrastructure.Data;

namespace Misil.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly MisilDbContext _context;

    public ChatRepository(MisilDbContext context) => _context = context;

    public async Task<List<ChatChannel>> GetChannelsAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        var userTeamIds = await _context.TeamMembers
            .Where(tm => tm.UserId == userId)
            .Select(tm => tm.TeamId)
            .ToListAsync(ct);

        return await _context.ChatChannels
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId && (c.TeamId == null || userTeamIds.Contains(c.TeamId.Value)))
            .ToListAsync(ct);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid channelId, int limit = 50, Guid? before = null, CancellationToken ct = default)
    {
        var query = _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.ChannelId == channelId);

        if (before.HasValue)
        {
            var beforeDate = await _context.ChatMessages
                .Where(m => m.Id == before.Value)
                .Select(m => m.CreatedAt)
                .FirstOrDefaultAsync(ct);
            query = query.Where(m => m.CreatedAt < beforeDate);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message, CancellationToken ct = default)
    {
        await _context.ChatMessages.AddAsync(message, ct);
        await _context.SaveChangesAsync(ct);
        return message;
    }

    public async Task<ChatChannel> CreateChannelAsync(ChatChannel channel, CancellationToken ct = default)
    {
        await _context.ChatChannels.AddAsync(channel, ct);
        await _context.SaveChangesAsync(ct);
        return channel;
    }

    public async Task MarkAsReadAsync(Guid userId, Guid messageId, CancellationToken ct = default)
    {
        var existing = await _context.ChatMessageReads
            .FindAsync([userId, messageId], ct);

        if (existing == null)
        {
            await _context.ChatMessageReads.AddAsync(new ChatMessageRead
            {
                UserId = userId,
                MessageId = messageId,
                ReadAt = DateTime.UtcNow
            }, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
