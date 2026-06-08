using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Chat.Commands;
public record SendMessageCommand(Guid ChannelId, Guid UserId, string Content) : IRequest<MessageDto>;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IChatRepository _chatRepo;

    public SendMessageCommandHandler(IChatRepository chatRepo) => _chatRepo = chatRepo;

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var message = new ChatMessage(request.ChannelId, request.UserId, request.Content);
        await _chatRepo.AddMessageAsync(message, ct);
        return new MessageDto(message.Id, message.ChannelId, message.UserId, message.Content, message.CreatedAt, false);
    }
}
