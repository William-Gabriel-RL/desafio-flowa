using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.Tests;

public class OrdensEndpointTests
{
    private readonly HttpClient _client = new WebApplicationFactory<Program>().CreateClient();

    [Fact]
    public async Task OrdemAceita_Retorna200NoFormatoEsperado()
    {
        var response = await _client.PostAsJsonAsync("/ordens",
            new { ativo = "PETR4", lado = "C", quantidade = 584, preco = 54.87 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("sucesso").GetBoolean());
        Assert.Equal(32044.08m, json.RootElement.GetProperty("exposicao_atual").GetDecimal());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("msg_erro").ValueKind);
    }

    [Fact]
    public async Task OrdemAcimaDoLimite_Retorna422ComErro()
    {
        var response = await _client.PostAsJsonAsync("/ordens",
            new { ativo = "VALE3", lado = "C", quantidade = 20_000, preco = 60.0 });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("sucesso").GetBoolean());
        Assert.Equal(0m, json.RootElement.GetProperty("exposicao_atual").GetDecimal());
        Assert.False(string.IsNullOrEmpty(json.RootElement.GetProperty("msg_erro").GetString()));
    }

    [Fact]
    public async Task JsonMalformado_Retorna400NoFormatoPadrao()
    {
        var response = await _client.PostAsync("/ordens",
            new StringContent("{ \"ativo\": ", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("sucesso").GetBoolean());
    }
}
