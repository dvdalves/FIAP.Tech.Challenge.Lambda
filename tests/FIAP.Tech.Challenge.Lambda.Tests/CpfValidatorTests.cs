using FIAP.Tech.Challenge.Lambda.Services;
using FluentAssertions;
using Xunit;

namespace FIAP.Tech.Challenge.Lambda.Tests;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("52998224725", "52998224725")]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("12345678909", "12345678909")]
    [InlineData("123.456.789-09", "12345678909")]
    public void Validar_ComCpfValido_DeveRetornarTrueECpfLimpo(string cpfEntrada, string cpfEsperado)
    {
        // Act
        var resultado = CpfValidator.Validar(cpfEntrada, out var cpfLimpo);

        // Assert
        resultado.Should().BeTrue();
        cpfLimpo.Should().Be(cpfEsperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("1234567890")] // 10 dígitos
    [InlineData("123456789012")] // 12 dígitos
    [InlineData("00000000000")] // Dígitos repetidos
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("99999999999")]
    [InlineData("12345678900")] // Primeiro dígito verificador errado
    [InlineData("12345678908")] // Segundo dígito verificador errado
    [InlineData("abcdefghijk")] // Não numérico
    public void Validar_ComCpfInvalido_DeveRetornarFalse(string? cpfInvalido)
    {
        // Act
        var resultado = CpfValidator.Validar(cpfInvalido, out var cpfLimpo);

        // Assert
        resultado.Should().BeFalse();
    }
}
