# Azure Container Apps fornece execução serverless com escala para zero para
# o serviço de autenticação serverless.

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.azure_region
  tags = {
    Project     = "FIAP-Tech-Challenge"
    Environment = var.environment
    ManagedBy   = "Terraform"
    Repository  = "FIAP.Tech.Challenge.Lambda"
  }
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "${var.function_name}-logs"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "main" {
  name                       = var.container_environment_name
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
}

resource "azurerm_container_app" "auth" {
  name                         = var.function_name
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  template {
    min_replicas = 0
    max_replicas = 5

    container {
      name   = "auth"
      image  = var.container_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "JWT_SECRET_KEY"
        secret_name = "jwt-secret"
      }
      env {
        name  = "JWT_EXPIRATION_HOURS"
        value = tostring(var.jwt_expiration_hours)
      }
      env {
        name        = "DB_CONNECTION_STRING"
        secret_name = "db-connection"
      }
      env {
        name  = "MOCK_DATABASE"
        value = var.mock_database
      }
    }
  }

  secret {
    name  = "jwt-secret"
    value = var.jwt_secret_key
  }
  secret {
    name  = "db-connection"
    value = var.db_connection_string
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "http"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}

output "auth_endpoint" {
  description = "URL pública do serviço serverless de autenticação"
  value       = "https://${azurerm_container_app.auth.latest_revision_fqdn}"
}
