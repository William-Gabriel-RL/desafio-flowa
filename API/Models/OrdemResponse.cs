using System.Text.Json.Serialization;

namespace API.Models;

public record OrdemResponse(
    [property: JsonPropertyName("sucesso")] bool Sucesso,
    [property: JsonPropertyName("exposicao_atual")] decimal ExposicaoAtual,
    [property: JsonPropertyName("msg_erro")] string? MsgErro);
