using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Chat.Queries;
public record GetChannelsQuery(Guid ProjectId, Guid UserId) : IRequest<List<ChannelDto>>;

public class GetChannelsQueryHandler : IRequestHandler<GetChannelsQuery, List<ChannelDto>>
{
    private readonly IChatRepository _chatRepo;

    public GetChannelsQueryHandler(IChatRepository chatRepo) => _chatRepo = chatRepo;

    public async Task<List<ChannelDto>> Handle(GetChannelsQuery request, CancellationToken ct)
    {
        var channels = await _chatRepo.GetChannelsAsync(request.ProjectId, request.UserId, ct);
        return channels.Select(c => new ChannelDto(c.Id, c.ProjectId, c.Name, c.TeamId, c.IsPrivate, c.CreatedAt)).ToList();
    }
}
