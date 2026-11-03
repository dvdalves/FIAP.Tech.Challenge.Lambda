using System.Text.Json;
using FIAP.Tech.Challenge.Lambda.Models;
using FIAP.Tech.Challenge.Lambda.Services;

namespace FIAP.Tech.Challenge.Lambda;

public class Function(
    IClienteDatabaseService databaseService,
    IJwtTokenService jwtTokenService)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<(int statusCode, string responseBody)> ProcessarRequisicaoAsync(string? requestBody)
    {
        if (string.IsNullOrWhiteSpace(requestBody))
            return Erro(400, "Corpo da requisição não pode ser vazio. Forneça o CPF no formato JSON: {\"cpf\": \"12345678909\"}.");

        AuthRequest? authRequest;
        try
        {
            authRequest = JsonSerializer.Deserialize<AuthRequest>(requestBody, JsonOptions);
        }
        catch (JsonException)
        {
            return Erro(400, "JSON inválido. Forneça um payload no formato {\"cpf\": \"...\"}.");
        }

        if (authRequest == null || string.IsNullOrWhiteSpace(authRequest.Cpf))
            return Erro(400, "O campo 'cpf' é obrigatório para autenticação.");

        if (!CpfValidator.Validar(authRequest.Cpf, out var cpfLimpo))
            return Erro(400, "CPF informado é inválido conforme as regras oficiais da Receita Federal.");

        ClienteAuthDto? cliente;
        try
        {
            cliente = await databaseService.ObterClientePorCpfAsync(cpfLimpo);
        }
        catch (Exception ex)
        {
            return Erro(500, $"Falha na comunicação com o banco de dados: {ex.Message}");
        }

        if (cliente == null)
            return Erro(404, "Cliente não encontrado na base de dados.");

        var token = jwtTokenService.GerarToken(cliente, out var expiresInSeconds);
        return (200, JsonSerializer.Serialize(new AuthResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = expiresInSeconds,
            Cliente = cliente
        }, JsonOptions));
    }

    private static (int statusCode, string responseBody) Erro(int statusCode, string mensagem) =>
        (statusCode, JsonSerializer.Serialize(new ErrorResponse
        {
            Mensagem = mensagem,
            StatusCode = statusCode
        }, JsonOptions));
}
