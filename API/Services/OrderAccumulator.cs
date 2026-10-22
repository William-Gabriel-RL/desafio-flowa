using System.Globalization;
using API.Models;

namespace API.Services;

public class OrderAccumulator
{
    public const decimal LimiteExposicao = 1_000_000m;
    public const int QuantidadeMaxima = 100_000;
    public const decimal PrecoMaximo = 1_000m;

    public static readonly IReadOnlySet<string> AtivosPermitidos = new HashSet<string> { "PETR4", "VALE3", "VIIA4" };

    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

    private readonly Dictionary<string, decimal> _exposicoes = new();
    private readonly Lock _lock = new();

    public OrdemResponse Processar(OrdemRequest ordem)
    {
        var erro = Validar(ordem);
        if (erro is not null)
            return new OrdemResponse(false, ObterExposicao(ordem.Ativo), erro);

        var valor = ordem.Preco * ordem.Quantidade;
        var delta = ordem.Lado == "C" ? valor : -valor;

        lock (_lock)
        {
            var atual = _exposicoes.GetValueOrDefault(ordem.Ativo!);
            var nova = atual + delta;

            if (Math.Abs(nova) > LimiteExposicao)
                return new OrdemResponse(false, atual, string.Create(Cultura,
                    $"Ordem rejeitada: exposição resultante de {nova:N2} ultrapassa o limite de {LimiteExposicao:N2} para {ordem.Ativo}."));

            _exposicoes[ordem.Ativo!] = nova;
            return new OrdemResponse(true, nova, null);
        }
    }

    public decimal ObterExposicao(string? ativo)
    {
        if (ativo is null)
            return 0m;

        lock (_lock)
            return _exposicoes.GetValueOrDefault(ativo);
    }

    public static string? Validar(OrdemRequest ordem)
    {
        if (ordem.Ativo is null || !AtivosPermitidos.Contains(ordem.Ativo))
            return "Ativo inválido.";
        if (ordem.Lado is not ("C" or "V"))
            return "Lado inválido. Valores permitidos: C, V.";
        if (ordem.Quantidade <= 0)
            return "Quantidade deve ser maior que zero.";
        if (ordem.Quantidade >= QuantidadeMaxima)
            return string.Create(Cultura, $"Quantidade deve ser menor que {QuantidadeMaxima:N0}.");
        if (ordem.Preco <= 0)
            return "Preço deve ser maior que zero.";
        if (ordem.Preco >= PrecoMaximo)
            return string.Create(Cultura, $"Preço deve ser menor que {PrecoMaximo:N2}.");
        if (ordem.Preco % 0.01m != 0)
            return "Preço deve ser múltiplo de 0,01 e maior que 0";
        return null;
    }
}
