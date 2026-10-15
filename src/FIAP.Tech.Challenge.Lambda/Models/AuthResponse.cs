namespace FIAP.Tech.Challenge.Lambda.Models;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public ClienteAuthDto Cliente { get; set; } = new();
}
