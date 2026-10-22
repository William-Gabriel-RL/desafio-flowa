namespace API.Models;

public record OrdemRequest(string? Ativo, string? Lado, int Quantidade, decimal Preco);
