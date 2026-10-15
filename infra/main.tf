# ==============================================================================
# 1. IAM ROLE & POLICIES PARA A LAMBDA
# ==============================================================================

resource "aws_iam_role" "lambda_exec" {
  name = "${var.function_name}-exec-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy_attachment" "lambda_vpc_access" {
  role       = aws_iam_role.lambda_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
}

# ==============================================================================
# 2. DUMMY ZIP (PARA PERMITIR TERRAFORM INIT / PLAN ANTES DO BUILD)
# ==============================================================================

data "archive_file" "dummy_payload" {
  type        = "zip"
  output_path = "${path.module}/dummy_payload.zip"

  source {
    content  = "Placeholder para build da aplicacao .NET"
    filename = "placeholder.txt"
  }
}

# ==============================================================================
# 3. AWS LAMBDA FUNCTION (SERVERLESS AUTHENTICATION)
# ==============================================================================

resource "aws_lambda_function" "auth_lambda" {
  function_name    = var.function_name
  role             = aws_iam_role.lambda_exec.arn
  handler          = "FIAP.Tech.Challenge.Lambda::FIAP.Tech.Challenge.Lambda.Function::FunctionHandlerAsync"
  runtime          = "dotnet8"
  filename         = data.archive_file.dummy_payload.output_path
  source_code_hash = data.archive_file.dummy_payload.output_base64sha256
  timeout          = 15
  memory_size      = 256

  environment {
    variables = {
      JWT_SECRET_KEY       = var.jwt_secret_key
      JWT_EXPIRATION_HOURS = tostring(var.jwt_expiration_hours)
      DB_CONNECTION_STRING = var.db_connection_string
      MOCK_DATABASE        = var.mock_database
    }
  }

  tags = {
    Name = var.function_name
  }
}

# ==============================================================================
# 4. AWS API GATEWAY (HTTP API v2)
# ==============================================================================

resource "aws_apigatewayv2_api" "auth_api" {
  name          = var.api_name
  protocol_type = "HTTP"

  cors_configuration {
    allow_origins = ["*"]
    allow_methods = ["POST", "OPTIONS"]
    allow_headers = ["Content-Type", "Authorization"]
    max_age       = 300
  }

  tags = {
    Name = var.api_name
  }
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.auth_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id                 = aws_apigatewayv2_api.auth_api.id
  integration_type       = "AWS_PROXY"
  integration_uri        = aws_lambda_function.auth_lambda.invoke_arn
  integration_method     = "POST"
  payload_format_version = "2.0"
}

resource "aws_apigatewayv2_route" "auth_token" {
  api_id    = aws_apigatewayv2_api.auth_api.id
  route_key = "POST /auth/token"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_route" "auth_login" {
  api_id    = aws_apigatewayv2_api.auth_api.id
  route_key = "POST /auth/login"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

# Permissão para o API Gateway invocar a Lambda
resource "aws_lambda_permission" "api_gateway" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.auth_lambda.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.auth_api.execution_arn}/*/*"
}
