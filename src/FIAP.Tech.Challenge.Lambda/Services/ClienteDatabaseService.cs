using FIAP.Tech.Challenge.Lambda.Models;
using Npgsql;

namespace FIAP.Tech.Challenge.Lambda.Services;

public class ClienteDatabaseService : IClienteDatabaseService
{
    private readonly string? _connectionString;
    private readonly bool _useMock;

    public ClienteDatabaseService(string? connectionString = null, bool? useMock = null)
    {
        _connectionString = connectionString ?? ObterConnectionString();
        _useMock = useMock ?? string.Equals(Environment.GetEnvironmentVariable("MOCK_DATABASE"), "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ClienteAuthDto?> ObterClientePorCpfAsync(string cpfLimpo, CancellationToken cancellationToken = default)
    {
        if (_useMock || string.IsNullOrWhiteSpace(_connectionString))
        {
            return ObterMockPorCpf(cpfLimpo);
        }

        try
        {
            await using var dataSource = NpgsqlDataSource.Create(_connectionString);
            await using var command = dataSource.CreateCommand(
                """
                SELECT "Id", "Nome", "Cpf", "Email", "Telefone"
                FROM "Clientes"
                WHERE "Cpf" = @cpf
                LIMIT 1;
                """);

            command.Parameters.AddWithValue("cpf", cpfLimpo);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new ClienteAuthDto
                {
                    Id = reader.GetGuid(0),
                    Nome = reader.GetString(1),
                    Cpf = reader.GetString(2),
                    Email = reader.GetString(3),
                    Telefone = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = "Ativo"
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AVISO] Erro ao conectar ao PostgreSQL: {ex.Message}. Verificando fallback de desenvolvimento.");
            if (string.Equals(Environment.GetEnvironmentVariable("ENABLE_DB_FALLBACK"), "true", StringComparison.OrdinalIgnoreCase))
            {
                return ObterMockPorCpf(cpfLimpo);
            }
            throw;
        }
    }

    private static ClienteAuthDto? ObterMockPorCpf(string cpfLimpo)
    {
        // Mock de clientes pré-cadastrados para testes e validações locais
        var mockDatabase = new Dictionary<string, ClienteAuthDto>
        {
            ["52998224725"] = new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Nome = "Carlos Alberto Silva (Teste)",
                Cpf = "52998224725",
                Email = "carlos.silva@example.com",
                Telefone = "11987654321",
                Status = "Ativo"
            },
            ["12345678909"] = new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Nome = "Ana Beatriz Souza (Teste)",
                Cpf = "12345678909",
                Email = "ana.souza@example.com",
                Telefone = "11912345678",
                Status = "Ativo"
            }
        };

        return mockDatabase.GetValueOrDefault(cpfLimpo);
    }

    private static string? ObterConnectionString()
    {
        var connStr = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connStr))
            return connStr;

        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "oficina_db";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "oficina_admin";
        var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(pass))
        {
            return $"Host={host};Port={port};Database={dbName};Username={user};Password={pass};Timeout=5;Command Timeout=5";
        }

        return null;
    }
}
