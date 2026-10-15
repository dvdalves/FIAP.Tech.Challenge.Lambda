using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using FIAP.Tech.Challenge.Lambda.Models;
using FIAP.Tech.Challenge.Lambda.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.Lambda.Tests;

public class FunctionHandlerTests
{
    private readonly IClienteDatabaseService _databaseService = Substitute.For<IClienteDatabaseService>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();

    private Function CreateSut()
    {
        return new Function(_databaseService, _jwtTokenService);
    }

    [Fact]
    public async Task ProcessarRequisicao_QuandoBodyVazio_DeveRetornar400()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var (statusCode, body) = await sut.ProcessarRequisicaoAsync(null);

        // Assert
        statusCode.Should().Be(400);
        body.Should().Contain("não pode ser vazio");
    }

    [Fact]
    public async Task ProcessarRequisicao_QuandoJsonInvalido_DeveRetornar400()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var (statusCode, body) = await sut.ProcessarRequisicaoAsync("json-invalido");

        // Assert
        statusCode.Should().Be(400);
        body.Should().Contain("JSON inválido");
    }

    [Fact]
    public async Task ProcessarRequisicao_QuandoCpfNaoInformado_DeveRetornar400()
    {
        // Arrange
        var sut = CreateSut();
        var requestJson = JsonSerializer.Serialize(new { cpf = "" });

        // Act
        var (statusCode, body) = await sut.ProcessarRequisicaoAsync(requestJson);

        // Assert
        statusCode.Should().Be(400);
        body.Should().Contain("obrigatório");
    }

    [Fact]
    public async Task ProcessarRequisicao_QuandoCpfInvalido_DeveRetornar400()
    {
        // Arrange
        var sut = CreateSut();
        var requestJson = JsonSerializer.Serialize(new { cpf = "111.111.111-11" });

        // Act
        var (statusCode, body) = await sut.ProcessarRequisicaoAsync(requestJson);

        // Assert
        statusCode.Should().Be(400);
        body.Should().Contain("CPF informado é inválido");
    }

    [Fact]
    public async Task ProcessarRequisicao_QuandoClienteNaoEncontradoNoBanco_DeveRetornar404()
    {
        // Arrange
        var sut = CreateSut();
        var cpfValido = "52998224725";
        var requestJson = JsonSerializer.Serialize(new { cpf = cpfValido });

        _databaseService.ObterClientePorCpfAsync(cpfValido).Returns(Task.FromResult<ClienteAuthDto?>(null));

        // Act
        var (statusCode, body) = await sut.ProcessarRequisicaoAsync(requestJson);

        // Assert
        statusCode.Should().Be(404);
        body.Should().Contain("não encontrado na base");
    }

    [Fact]
    public async Task ProcessarRequisicao_QuandoClienteValido_DeveRetornar200ETokenJwt()
    {
        // Arrange
        var sut = CreateSut();
        var cpfValido = "52998224725";
        var requestJson = JsonSerializer.Serialize(new { cpf = cpfValido });

        var cliente = new ClienteAuthDto
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Teste",
            Cpf = cpfValido,
            Email = "carlos@teste.com",
            Status = "Ativo"
        };

        _databaseService.ObterClientePorCpfAsync(cpfValido).Returns(Task.FromResult<ClienteAuthDto?>(cliente));
        _jwtTokenService.GerarToken(cliente, out Arg.Any<int>())
            .Returns(x =>
            {
                x[1] = 7200;
                return "mock-jwt-token-12345";
            });

        // Act
        var (statusCode, body) = await sut.ProcessarRequisicaoAsync(requestJson);

        // Assert
        statusCode.Should().Be(200);
        var response = JsonSerializer.Deserialize<AuthResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        response.Should().NotBeNull();
        response!.Token.Should().Be("mock-jwt-token-12345");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(7200);
        response.Cliente.Nome.Should().Be("Carlos Teste");
    }

    [Fact]
    public async Task FunctionHandlerAsync_ComPayloadV2_DeveRetornarHeadersCorsERespostaCorreta()
    {
        // Arrange
        var sut = CreateSut();
        var cpfValido = "52998224725";
        var request = new APIGatewayHttpApiV2ProxyRequest
        {
            Body = JsonSerializer.Serialize(new { cpf = cpfValido })
        };

        var cliente = new ClienteAuthDto
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Teste",
            Cpf = cpfValido,
            Email = "carlos@teste.com"
        };

        _databaseService.ObterClientePorCpfAsync(cpfValido).Returns(Task.FromResult<ClienteAuthDto?>(cliente));
        _jwtTokenService.GerarToken(cliente, out Arg.Any<int>())
            .Returns(x =>
            {
                x[1] = 7200;
                return "mock-jwt-token-v2";
            });

        // Act
        var response = await sut.FunctionHandlerAsync(request);

        // Assert
        response.StatusCode.Should().Be(200);
        response.Headers["Content-Type"].Should().Be("application/json");
        response.Headers["Access-Control-Allow-Origin"].Should().Be("*");
        response.Body.Should().Contain("mock-jwt-token-v2");
    }
}
