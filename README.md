# FIAP Tech Challenge — Serviço de Autenticação

[![CI/CD Pipeline](https://github.com/dvdalves/FIAP.Tech.Challenge.Lambda/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/dvdalves/FIAP.Tech.Challenge.Lambda/actions/workflows/ci-cd.yml)

Serviço responsável pela autenticação de clientes por CPF. A aplicação valida o CPF, consulta o cliente no PostgreSQL e emite um token JWT para uso pela API principal.

## Sumário

1. [Responsabilidade](#1-responsabilidade)
2. [Tecnologias](#2-tecnologias)
3. [API](#3-api)
4. [Como Executar](#4-como-executar)
5. [Infraestrutura](#5-infraestrutura)
6. [CI/CD](#6-cicd)

## 1. Responsabilidade

- Validar o CPF usando o algoritmo oficial de dígitos verificadores.
- Consultar a existência e o status do cliente no PostgreSQL.
- Emitir tokens JWT assinados para autenticação na API principal.

## 2. Tecnologias

- .NET 10 e C#.
- Minimal API.
- PostgreSQL com Npgsql.
- JWT com HMAC-SHA256.
- Docker.
- Terraform para Azure Container Apps e Azure API Management.
- xUnit, FluentAssertions e NSubstitute.

## 3. API

### `POST /auth/token`

Request:

```json
{
  "cpf": "52998224725"
}
```

O CPF também pode ser enviado com máscara.

| Status | Significado |
|---|---|
| `200` | CPF válido, cliente encontrado e token emitido. |
| `400` | CPF inválido ou corpo da requisição inválido. |
| `404` | Cliente não encontrado. |
| `500` | Falha inesperada na consulta. |

## 4. Como Executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- Docker Desktop, se optar pela execução em contêiner.
- PostgreSQL, se executar sem o modo mock.

### Execução local

Para testes isolados sem banco:

```bash
MOCK_DATABASE=true dotnet run --project src/FIAP.Tech.Challenge.Lambda
```

### Execução via Docker

```bash
docker build -t oficina-auth .
docker run --rm -p 8080:8080 \
  -e MOCK_DATABASE=true \
  oficina-auth
```

### Testes

```bash
dotnet test FIAP.Tech.Challenge.Lambda.slnx --verbosity normal
```

## 5. Infraestrutura

Os arquivos Terraform estão em [`infra/`](infra/). Configure as variáveis por arquivo local ou pelo pipeline; não publique arquivos `.tfvars` com credenciais.

```bash
cd infra
terraform init
terraform fmt -check
terraform validate
terraform plan
```

O segredo usado para assinar JWT deve ser fornecido por variável segura (`JWT_SECRET_KEY`) ou pelo mecanismo de secrets do ambiente. Não há segredo padrão válido no código.

## 6. CI/CD

O workflow [`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml) executa testes, validação do Terraform e build da imagem Docker. O deploy depende das configurações e secrets do ambiente Azure.
