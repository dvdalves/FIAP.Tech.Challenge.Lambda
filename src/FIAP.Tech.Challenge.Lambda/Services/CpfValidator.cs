using System.Text.RegularExpressions;

namespace FIAP.Tech.Challenge.Lambda.Services;

public static class CpfValidator
{
    public static bool Validar(string? cpf, out string cpfLimpo)
    {
        cpfLimpo = string.Empty;

        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Extrair apenas dígitos numéricos
        cpfLimpo = Regex.Replace(cpf, @"[^\d]", "");

        if (cpfLimpo.Length != 11)
            return false;

        // Rejeitar sequências conhecidas de dígitos repetidos (ex: 00000000000, 11111111111, ...)
        if (new string(cpfLimpo[0], 11) == cpfLimpo)
            return false;

        // Cálculo do 1º Dígito Verificador
        int[] multiplicadores1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int soma = 0;

        for (int i = 0; i < 9; i++)
        {
            soma += (cpfLimpo[i] - '0') * multiplicadores1[i];
        }

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        if (cpfLimpo[9] - '0' != digito1)
            return false;

        // Cálculo do 2º Dígito Verificador
        int[] multiplicadores2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];
        soma = 0;

        for (int i = 0; i < 10; i++)
        {
            soma += (cpfLimpo[i] - '0') * multiplicadores2[i];
        }

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cpfLimpo[10] - '0' == digito2;
    }
}
