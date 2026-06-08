using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Chat.Queries;
public record GetMessagesQuery(Guid ChannelId, int Limit = 50, Guid? Before = null) : IRequest<List<MessageDto>>;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, List<MessageDto>>
{
    private readonly IChatRepository _chatRepo;

    public GetMessagesQueryHandler(IChatRepository chatRepo) => _chatRepo = chatRepo;

    public async Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        var messages = await _chatRepo.GetMessagesAsync(request.ChannelId, request.Limit, request.Before, ct);
        return messages.Select(m => new MessageDto(m.Id, m.ChannelId, m.UserId, m.Content, m.CreatedAt, m.IsRead)).ToList();
    }
}
