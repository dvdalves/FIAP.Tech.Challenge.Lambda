using FIAP.Tech.Challenge.Lambda.Models;

namespace FIAP.Tech.Challenge.Lambda.Services;

public interface IClienteDatabaseService
{
    Task<ClienteAuthDto?> ObterClientePorCpfAsync(string cpfLimpo, CancellationToken cancellationToken = default);
}
