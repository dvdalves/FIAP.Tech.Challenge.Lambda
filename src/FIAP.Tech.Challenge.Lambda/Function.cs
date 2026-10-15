using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FIAP.Tech.Challenge.Lambda.Models;
using FIAP.Tech.Challenge.Lambda.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FIAP.Tech.Challenge.Lambda;

public class Function
{
    private readonly IClienteDatabaseService _databaseService;
    private readonly IJwtTokenService _jwtTokenService;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public Function() : this(new ClienteDatabaseService(), new JwtTokenService())
    {
    }

    public Function(IClienteDatabaseService databaseService, IJwtTokenService jwtTokenService)
    {
        _databaseService = databaseService;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Handler para AWS API Gateway HTTP API (Payload v2.0)
    /// </summary>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandlerAsync(
        APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext? context = null)
    {
        context?.Logger.LogLine($"Recebida requisição HTTP API v2: {request.RequestContext?.Http?.Method} {request.RequestContext?.Http?.Path}");

        var (statusCode, responseBody) = await ProcessarRequisicaoAsync(request.Body);

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = statusCode,
            Body = responseBody,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "POST, OPTIONS" },
                { "Access-Control-Allow-Headers", "Content-Type, Authorization" }
            }
        };
    }

    /// <summary>
    /// Handler para AWS API Gateway REST API (Payload v1.0)
    /// </summary>
    public async Task<APIGatewayProxyResponse> FunctionHandlerV1Async(
        APIGatewayProxyRequest request,
        ILambdaContext? context = null)
    {
        context?.Logger.LogLine($"Recebida requisição REST API v1: {request.HttpMethod} {request.Path}");

        var (statusCode, responseBody) = await ProcessarRequisicaoAsync(request.Body);

        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = responseBody,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" }
            }
        };
    }

    /// <summary>
    /// Processa o fluxo de autenticação por CPF
    /// </summary>
    public async Task<(int statusCode, string responseBody)> ProcessarRequisicaoAsync(string? requestBody)
    {
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return (400, JsonSerializer.Serialize(new ErrorResponse
            {
                Mensagem = "Corpo da requisição não pode ser vazio. Forneça o CPF no formato JSON: {\"cpf\": \"12345678909\"}.",
                StatusCode = 400
            }, JsonOptions));
        }

        AuthRequest? authRequest;
        try
        {
            authRequest = JsonSerializer.Deserialize<AuthRequest>(requestBody, JsonOptions);
        }
        catch (Exception)
        {
            return (400, JsonSerializer.Serialize(new ErrorResponse
            {
                Mensagem = "JSON inválido. Forneça um payload no formato {\"cpf\": \"...\"}.",
                StatusCode = 400
            }, JsonOptions));
        }

        if (authRequest == null || string.IsNullOrWhiteSpace(authRequest.Cpf))
        {
            return (400, JsonSerializer.Serialize(new ErrorResponse
            {
                Mensagem = "O campo 'cpf' é obrigatório para autenticação.",
                StatusCode = 400
            }, JsonOptions));
        }

        // 1. Validar o algoritmo do CPF
        if (!CpfValidator.Validar(authRequest.Cpf, out var cpfLimpo))
        {
            return (400, JsonSerializer.Serialize(new ErrorResponse
            {
                Mensagem = "CPF informado é inválido conforme as regras oficiais da Receita Federal.",
                StatusCode = 400
            }, JsonOptions));
        }

        // 2. Consultar a existência e o status do cliente na base de dados
        ClienteAuthDto? cliente;
        try
        {
            cliente = await _databaseService.ObterClientePorCpfAsync(cpfLimpo);
        }
        catch (Exception ex)
        {
            return (500, JsonSerializer.Serialize(new ErrorResponse
            {
                Mensagem = $"Falha na comunicação com o banco de dados: {ex.Message}",
                StatusCode = 500
            }, JsonOptions));
        }

        if (cliente == null)
        {
            return (404, JsonSerializer.Serialize(new ErrorResponse
            {
                Mensagem = "Cliente não encontrado na base de dados. Cadastre o cliente previamente antes de solicitar autenticação.",
                StatusCode = 404
            }, JsonOptions));
        }

        // 3. Gerar e devolver token JWT válido
        var token = _jwtTokenService.GerarToken(cliente, out var expiresInSeconds);

        var response = new AuthResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = expiresInSeconds,
            Cliente = cliente
        };

        return (200, JsonSerializer.Serialize(response, JsonOptions));
    }
}
