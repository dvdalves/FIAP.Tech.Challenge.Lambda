using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FIAP.Tech.Challenge.Lambda.Models;
using Microsoft.IdentityModel.Tokens;

namespace FIAP.Tech.Challenge.Lambda.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secretKey;
    private readonly int _expirationHours;

    public JwtTokenService(string? secretKey = null, int? expirationHours = null)
    {
        _secretKey = secretKey
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? "SuperSecretSecurityKeyOficinaMecanica2026!";

        var envExp = Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS");
        _expirationHours = expirationHours
            ?? (int.TryParse(envExp, out var exp) ? exp : 2);
    }

    public string GerarToken(ClienteAuthDto cliente, out int expiresInSeconds)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        expiresInSeconds = _expirationHours * 3600;
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
                // ClaimTypes.Name com o CPF para atender à validação dos controllers da aplicação principal
                new Claim(ClaimTypes.Name, cliente.Cpf),
                new Claim("client_id", cliente.Id.ToString()),
                new Claim("cpf", cliente.Cpf),
                new Claim("client_name", cliente.Nome),
                new Claim(ClaimTypes.Email, cliente.Email),
                new Claim(ClaimTypes.Role, "Cliente"),
                new Claim("status", cliente.Status)
            ]),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
