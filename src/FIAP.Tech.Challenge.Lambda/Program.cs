using System.Text.Json;
using FIAP.Tech.Challenge.Lambda;
using FIAP.Tech.Challenge.Lambda.Models;
using FIAP.Tech.Challenge.Lambda.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClienteDatabaseService, ClienteDatabaseService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<Function>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "FIAP.Tech.Challenge.Lambda", timestamp = DateTime.UtcNow }));

app.MapPost("/auth/token", async (AuthRequest request, Function function) =>
{
    var rawJson = JsonSerializer.Serialize(request);
    var (status, body) = await function.ProcessarRequisicaoAsync(rawJson);
    return Results.Content(body, "application/json", statusCode: status);
});

app.MapPost("/auth/login", async (AuthRequest request, Function function) =>
{
    var rawJson = JsonSerializer.Serialize(request);
    var (status, body) = await function.ProcessarRequisicaoAsync(rawJson);
    return Results.Content(body, "application/json", statusCode: status);
});

app.Run();
