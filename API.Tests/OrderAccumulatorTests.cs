using System.Globalization;
using API.Models;
using API.Services;

namespace API.Tests;

public class OrderAccumulatorTests
{
    private readonly OrderAccumulator _accumulator = new();

    [Fact]
    public void Compra_AumentaExposicao()
    {
        var resposta = _accumulator.Processar(new OrdemRequest("PETR4", "C", 584, 54.87m));

        Assert.True(resposta.Sucesso);
        Assert.Equal(32044.08m, resposta.ExposicaoAtual);
        Assert.Null(resposta.MsgErro);
    }

    [Fact]
    public void Venda_DiminuiExposicao()
    {
        _accumulator.Processar(new OrdemRequest("PETR4", "C", 100, 10m));
        var resposta = _accumulator.Processar(new OrdemRequest("PETR4", "V", 30, 10m));

        Assert.True(resposta.Sucesso);
        Assert.Equal(700m, resposta.ExposicaoAtual);
    }

    [Fact]
    public void Venda_PodeDeixarExposicaoNegativa()
    {
        var resposta = _accumulator.Processar(new OrdemRequest("VALE3", "V", 10, 50m));

        Assert.True(resposta.Sucesso);
        Assert.Equal(-500m, resposta.ExposicaoAtual);
    }

    [Fact]
    public void ExposicaoEhIndependentePorAtivo()
    {
        _accumulator.Processar(new OrdemRequest("PETR4", "C", 10_000, 100m));
        var resposta = _accumulator.Processar(new OrdemRequest("VALE3", "C", 10_000, 100m));

        Assert.True(resposta.Sucesso);
        Assert.Equal(1_000_000m, _accumulator.ObterExposicao("PETR4"));
        Assert.Equal(1_000_000m, _accumulator.ObterExposicao("VALE3"));
    }

    [Fact]
    public void ExposicaoIgualAoLimite_EhAceita()
    {
        var resposta = _accumulator.Processar(new OrdemRequest("PETR4", "C", 10_000, 100m));

        Assert.True(resposta.Sucesso);
        Assert.Equal(1_000_000m, resposta.ExposicaoAtual);
    }

    [Theory]
    [InlineData("C")]
    [InlineData("V")]
    public void UltrapassarLimite_RejeitaENaoRegistra(string lado)
    {
        _accumulator.Processar(new OrdemRequest("VIIA4", lado, 9_000, 100m));
        var resposta = _accumulator.Processar(new OrdemRequest("VIIA4", lado, 1_001, 100m));

        var esperado = lado == "C" ? 900_000m : -900_000m;
        Assert.False(resposta.Sucesso);
        Assert.Equal(esperado, resposta.ExposicaoAtual);
        Assert.NotNull(resposta.MsgErro);
        Assert.Equal(esperado, _accumulator.ObterExposicao("VIIA4"));
    }

    [Fact]
    public void OrdemQueReduzExposicao_EhAceitaMesmoPertoDoLimite()
    {
        _accumulator.Processar(new OrdemRequest("PETR4", "C", 10_000, 100m));
        var resposta = _accumulator.Processar(new OrdemRequest("PETR4", "V", 19_000, 100m));

        Assert.True(resposta.Sucesso);
        Assert.Equal(-900_000m, resposta.ExposicaoAtual);
    }

    [Theory]
    [InlineData("ITUB4", "C", 1, 1)]
    [InlineData(null, "C", 1, 1)]
    [InlineData("PETR4", "X", 1, 1)]
    [InlineData("PETR4", "c", 1, 1)]
    [InlineData("PETR4", "C", 0, 1)]
    [InlineData("PETR4", "C", -5, 1)]
    [InlineData("PETR4", "C", 1, 0)]
    [InlineData("PETR4", "C", 1, -1)]
    [InlineData("PETR4", "C", 100_000, 1)]
    [InlineData("PETR4", "C", 1, 1_000)]
    [InlineData("PETR4", "C", 1, 10.001)]
    [InlineData("PETR4", "C", 1, 0.005)]
    public void OrdemInvalida_RejeitaENaoRegistra(string? ativo, string lado, int quantidade, decimal preco)
    {
        var resposta = _accumulator.Processar(new OrdemRequest(ativo, lado, quantidade, preco));

        Assert.False(resposta.Sucesso);
        Assert.NotNull(resposta.MsgErro);
        Assert.Equal(0m, _accumulator.ObterExposicao("PETR4"));
    }

    [Theory]
    [InlineData(99_999, 0.01)]
    [InlineData(1, 999.99)]
    [InlineData(1, 0.29)]
    public void OrdemNosLimitesDosCampos_EhAceita(int quantidade, decimal preco)
    {
        var resposta = _accumulator.Processar(new OrdemRequest("PETR4", "C", quantidade, preco));

        Assert.True(resposta.Sucesso);
        Assert.Equal(quantidade * preco, resposta.ExposicaoAtual);
    }

    [Fact]
    public void MensagemDeErro_UsaFormatoBrasileiroIndependenteDaCulturaDoServidor()
    {
        var culturaOriginal = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        try
        {
            var resposta = _accumulator.Processar(new OrdemRequest("PETR4", "C", 20_000, 60m));

            Assert.Contains("1.200.000,00", resposta.MsgErro);
            Assert.Contains("1.000.000,00", resposta.MsgErro);
        }
        finally
        {
            CultureInfo.CurrentCulture = culturaOriginal;
        }
    }

    [Fact]
    public void OrdensConcorrentes_NaoUltrapassamLimite()
    {
        Parallel.For(0, 2_000, _ => _accumulator.Processar(new OrdemRequest("PETR4", "C", 10, 100m)));

        Assert.Equal(1_000_000m, _accumulator.ObterExposicao("PETR4"));
    }
}
