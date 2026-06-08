using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Misil.Application.Common.Interfaces;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Auth.Commands;
public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepo, IJwtService jwtService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, ct);
        if (user == null) throw new UnauthorizedAccessException("Invalid credentials");

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash!, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        var (token, refreshToken) = _jwtService.GenerateToken(user);
        return new AuthResponse(token, refreshToken, user.Adapt<UserDto>());
    }
}
