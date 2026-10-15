output "api_gateway_endpoint" {
  description = "URL base do AWS API Gateway para chamadas de autenticação"
  value       = aws_apigatewayv2_stage.default.invoke_url
}

output "auth_token_url" {
  description = "URL completa para emissão de token via CPF"
  value       = "${aws_apigatewayv2_stage.default.invoke_url}auth/token"
}

output "lambda_function_arn" {
  description = "ARN da AWS Lambda Function"
  value       = aws_lambda_function.auth_lambda.arn
}

output "lambda_function_name" {
  description = "Nome da AWS Lambda Function"
  value       = aws_lambda_function.auth_lambda.function_name
}
