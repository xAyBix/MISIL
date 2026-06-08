namespace Misil.Application.Auth;
public record AuthResponse(string Token, string RefreshToken, UserDto User);
public record UserDto(Guid Id, string Username, string Email, string? DisplayName, string? AvatarUrl);
