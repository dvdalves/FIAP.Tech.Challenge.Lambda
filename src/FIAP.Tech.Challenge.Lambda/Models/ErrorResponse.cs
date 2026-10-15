namespace FIAP.Tech.Challenge.Lambda.Models;

public class ErrorResponse
{
    public string Mensagem { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
