using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Misil.Application.Common.Interfaces;
using Misil.Domain.Entities;
using Misil.Domain.Interfaces;

namespace Misil.Application.Auth.Commands;
public record RegisterCommand(string Email, string Username, string Password, string? DisplayName = null) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUserRepository userRepo, IJwtService jwtService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email, ct);
        if (existing != null) throw new Exception("Email already registered");

        var user = new User(request.Username, request.Email, request.DisplayName);
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        await _userRepo.AddAsync(user, ct);

        var (token, refreshToken) = _jwtService.GenerateToken(user);
        return new AuthResponse(token, refreshToken, user.Adapt<UserDto>());
    }
}
