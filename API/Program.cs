using API.Models;
using API.Services;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<OrderAccumulator>();
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);

var app = builder.Build();

app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var badRequest = ex is BadHttpRequestException;
    context.Response.StatusCode = badRequest ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
    var msg = badRequest ? "Requisição inválida: corpo JSON malformado." : "Erro interno ao processar a ordem.";
    await context.Response.WriteAsJsonAsync(new OrdemResponse(false, 0m, msg));
}));

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.MapPost("/ordens", (OrdemRequest ordem, OrderAccumulator accumulator) =>
{
    var resposta = accumulator.Processar(ordem);
    return resposta.Sucesso ? Results.Ok(resposta) : Results.UnprocessableEntity(resposta);
});

app.Run();