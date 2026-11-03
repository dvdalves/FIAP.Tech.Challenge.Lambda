output "auth_service_endpoint" {
  description = "URL base do serviço serverless de autenticação"
  value       = "https://${azurerm_container_app.auth.latest_revision_fqdn}"
}

output "auth_token_url" {
  description = "URL completa para emissão de token via CPF"
  value       = "https://${azurerm_container_app.auth.latest_revision_fqdn}/auth/token"
}

output "container_app_id" {
  description = "ID do serviço serverless de autenticação"
  value       = azurerm_container_app.auth.id
}

output "container_app_environment_id" {
  description = "ID do ambiente Azure Container Apps"
  value       = azurerm_container_app_environment.main.id
}
