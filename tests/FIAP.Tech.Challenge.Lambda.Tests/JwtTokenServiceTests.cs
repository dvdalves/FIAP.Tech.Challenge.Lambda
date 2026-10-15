using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FIAP.Tech.Challenge.Lambda.Models;
using FIAP.Tech.Challenge.Lambda.Services;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace FIAP.Tech.Challenge.Lambda.Tests;

public class JwtTokenServiceTests
{
    private const string SecretKey = "TestSecretKeyOficinaMecanicaSuperSegura2026!";

    [Fact]
    public void GerarToken_ComClienteValido_DeveRetornarJwtValidoEClaimsEsperadas()
    {
        // Arrange
        var service = new JwtTokenService(secretKey: SecretKey, expirationHours: 2);
        var cliente = new ClienteAuthDto
        {
            Id = Guid.NewGuid(),
            Nome = "Maria Santos",
            Cpf = "52998224725",
            Email = "maria@exemplo.com",
            Telefone = "11999998888",
            Status = "Ativo"
        };

        // Act
        var tokenString = service.GerarToken(cliente, out var expiresInSeconds);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();
        expiresInSeconds.Should().Be(7200);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(tokenString, validationParameters, out var validatedToken);
        validatedToken.Should().NotBeNull();

        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(cliente.Id.ToString());
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be(cliente.Cpf);
        principal.FindFirst("cpf")?.Value.Should().Be(cliente.Cpf);
        principal.FindFirst("client_name")?.Value.Should().Be(cliente.Nome);
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(cliente.Email);
        principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Cliente");
        principal.FindFirst("status")?.Value.Should().Be("Ativo");
    }
}
