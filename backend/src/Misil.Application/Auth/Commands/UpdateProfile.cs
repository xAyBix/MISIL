using Mapster;
using MediatR;
using Misil.Application.Common.Interfaces;
using Misil.Domain.Interfaces;

namespace Misil.Application.Auth.Commands;
public record UpdateProfileCommand(Guid UserId, string? DisplayName, string? AvatarUrl) : IRequest<UserDto>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IUserRepository _userRepo;

    public UpdateProfileCommandHandler(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user is null) throw new Exception("User not found");

        if (request.DisplayName is not null) user.DisplayName = request.DisplayName;
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;

        await _userRepo.UpdateAsync(user, ct);
        return user.Adapt<UserDto>();
    }
}
