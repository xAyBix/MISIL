using Mapster;
using MediatR;
using Misil.Domain.Interfaces;

namespace Misil.Application.Auth.Queries;
public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _userRepo;

    public GetCurrentUserQueryHandler(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user.Adapt<UserDto>();
    }
}
