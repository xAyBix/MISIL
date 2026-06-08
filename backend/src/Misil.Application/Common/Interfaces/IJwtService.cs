namespace Misil.Application.Common.Interfaces;

public interface IJwtService
{
    (string Token, string RefreshToken) GenerateToken(Domain.Entities.User user);
}
