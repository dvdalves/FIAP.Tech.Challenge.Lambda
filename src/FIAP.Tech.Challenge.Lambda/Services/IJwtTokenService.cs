using FIAP.Tech.Challenge.Lambda.Models;

namespace FIAP.Tech.Challenge.Lambda.Services;

public interface IJwtTokenService
{
    string GerarToken(ClienteAuthDto cliente, out int expiresInSeconds);
}
