variable "aws_region" {
  description = "Região da AWS para deploy dos recursos"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Ambiente de deploy (ex: homolog, production)"
  type        = string
  default     = "production"
}

variable "function_name" {
  description = "Nome da função AWS Lambda"
  type        = string
  default     = "oficina-auth-lambda"
}

variable "jwt_secret_key" {
  description = "Chave secreta para assinatura dos tokens JWT"
  type        = string
  sensitive   = true
  default     = "SuperSecretSecurityKeyOficinaMecanica2026!"
}

variable "jwt_expiration_hours" {
  description = "Tempo de expiração do token JWT em horas"
  type        = number
  default     = 2
}

variable "db_connection_string" {
  description = "Connection string do banco de dados PostgreSQL RDS"
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
  description = "Nome do API Gateway HTTP API"
  type        = string
  default     = "oficina-auth-api-gateway"
}
