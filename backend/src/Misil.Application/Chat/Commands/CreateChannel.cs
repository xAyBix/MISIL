using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Chat.Commands;
public record CreateChannelCommand(Guid ProjectId, string Name, Guid? TeamId, bool IsPrivate) : IRequest<ChannelDto>;

public class CreateChannelCommandHandler : IRequestHandler<CreateChannelCommand, ChannelDto>
{
    private readonly IChatRepository _chatRepo;

    public CreateChannelCommandHandler(IChatRepository chatRepo) => _chatRepo = chatRepo;

    public async Task<ChannelDto> Handle(CreateChannelCommand request, CancellationToken ct)
    {
        var channel = new ChatChannel(request.ProjectId, request.Name, request.TeamId, request.IsPrivate);
        await _chatRepo.CreateChannelAsync(channel, ct);
        return new ChannelDto(channel.Id, channel.ProjectId, channel.Name, channel.TeamId, channel.IsPrivate, channel.CreatedAt);
    }
}
