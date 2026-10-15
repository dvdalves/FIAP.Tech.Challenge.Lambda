# ==============================================================================
# Multi-stage Dockerfile - FIAP.Tech.Challenge.Lambda (Function Serverless)
# ==============================================================================

# Estágio 1: Build & Publicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/FIAP.Tech.Challenge.Lambda/FIAP.Tech.Challenge.Lambda.csproj", "src/FIAP.Tech.Challenge.Lambda/"]
RUN dotnet restore "src/FIAP.Tech.Challenge.Lambda/FIAP.Tech.Challenge.Lambda.csproj"

COPY . .
WORKDIR "/src/src/FIAP.Tech.Challenge.Lambda"
RUN dotnet publish "FIAP.Tech.Challenge.Lambda.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

# Usuário não privilegiado para conformidade com segurança (CIS/OWASP)
USER $APP_UID

ENTRYPOINT ["dotnet", "FIAP.Tech.Challenge.Lambda.dll"]
