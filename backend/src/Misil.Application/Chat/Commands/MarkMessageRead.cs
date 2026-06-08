using MediatR;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Chat.Commands;
public record MarkMessageReadCommand(Guid UserId, Guid MessageId) : IRequest;

public class MarkMessageReadCommandHandler : IRequestHandler<MarkMessageReadCommand>
{
    private readonly IChatRepository _chatRepo;

    public MarkMessageReadCommandHandler(IChatRepository chatRepo) => _chatRepo = chatRepo;

    public async Task Handle(MarkMessageReadCommand request, CancellationToken ct)
    {
        await _chatRepo.MarkAsReadAsync(request.UserId, request.MessageId, ct);
    }
}
