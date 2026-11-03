variable "azure_region" {
  description = "Região do Azure para deploy dos recursos"
  type        = string
  default     = "East US"
}

variable "resource_group_name" {
  description = "Resource Group do Azure"
  type        = string
  default     = "fiap-tech-challenge-rg"
}

variable "environment" {
  description = "Ambiente de deploy (ex: homolog, production)"
  type        = string
  default     = "production"
}

variable "function_name" {
  description = "Nome do serviço serverless de autenticação no Azure Container Apps"
  type        = string
  default     = "oficina-auth-auth"
}

variable "container_environment_name" {
  description = "Nome do ambiente Azure Container Apps"
  type        = string
  default     = "oficina-auth-env"
}

variable "container_image" {
  description = "Imagem OCI da autenticação publicada em um registry"
  type        = string
  default     = "mcr.microsoft.com/dotnet/aspnet:8.0"
}

variable "jwt_secret_key" {
  description = "Chave secreta para assinatura dos tokens JWT"
  type        = string
  sensitive   = true
  default     = ""
}

variable "jwt_expiration_hours" {
  description = "Tempo de expiração do token JWT em horas"
  type        = number
  default     = 2
}

variable "db_connection_string" {
  description = "Connection string do Azure Database for PostgreSQL"
  type        = string
  sensitive   = true
  default     = ""
}

variable "mock_database" {
  description = "Habilitar modo mock de banco de dados para testes"
  type        = string
  default     = "false"
}

variable "api_name" {
  description = "Nome da API de autenticação no Azure API Management"
  type        = string
  default     = "oficina-auth-api"
}
