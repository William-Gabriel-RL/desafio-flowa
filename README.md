# Desafio Flowa - OrderGenerator e OrderAccumulator

Sistema de envio de ordens de compra e venda de ações, com controle de exposição financeira por ativo limitado a R$ 1.000.000.

>  This is a challenge by [Coodesh](https://coodesh.com/)

## Tecnologias

- **Backend (OrderAccumulator):** C# / .NET 10, ASP.NET Core Minimal API
- **Frontend (OrderGenerator):** Angular 21, TypeScript, Reactive Forms
- **Testes:** xUnit + WebApplicationFactory (backend), Vitest (frontend)

## Estrutura

```
API/         OrderAccumulator - API REST que recebe as ordens e calcula a exposição
API.Tests/   Testes unitários e de integração da API
Front/       OrderGenerator - página web com o formulário de ordens
```

## Como rodar

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19+ (testado com 24) e npm

### Backend

```bash
cd API
dotnet run
```

A API sobe em `http://localhost:5197`, porta para a qual o frontend aponta.

O arquivo `API/API.http` tem exemplos de requisições prontas para usar no VS Code ou Visual Studio.

### Frontend

Com a API rodando, em outro terminal:

```bash
cd Front
npm install
npm start
```

Acesse `http://localhost:4200`. As chamadas para `/ordens` são encaminhadas para a API pelo proxy do Angular (`proxy.conf.json`), então não é preciso configurar CORS.

### Testes

```bash
# backend
dotnet test API.Tests

# frontend
cd Front
npm test -- --watch=false
```

## API

### `POST /ordens`

Requisição:

```json
{
  "ativo": "PETR4",
  "lado": "C",
  "quantidade": 584,
  "preco": 54.87
}
```

| Campo        | Regra                                                  |
|--------------|--------------------------------------------------------|
| `ativo`      | `PETR4`, `VALE3` ou `VIIA4`                            |
| `lado`       | `C` (compra) ou `V` (venda)                            |
| `quantidade` | inteiro maior que 0 e menor que 100.000                |
| `preco`      | decimal maior que 0, menor que 1.000, múltiplo de 0,01 |

Resposta:

```json
{
  "sucesso": true,
  "exposicao_atual": 32044.08,
  "msg_erro": null
}
```

| Status | Quando                                                               |
|--------|----------------------------------------------------------------------|
| 200    | Ordem aceita e registrada na exposição                               |
| 400    | Campo inválido, JSON malformado ou campo com tipo incorreto          |
| 422    | Ordem válida, mas rejeitada por ultrapassar o limite de exposição    |

Em todos os casos o corpo segue o formato acima. Em um erro, `exposicao_atual` traz a exposição do ativo sem alteração (ou `0` quando não é possível identificar o ativo, como em um JSON malformado).

## Regras de negócio

- **Exposição financeira** por ativo = soma de (preço x quantidade) das compras - soma de (preço x quantidade) das vendas.
- O limite é de **R$ 1.000.000 por ativo, em valor absoluto**. Uma ordem que deixaria a exposição acima disso (positiva ou negativa) é rejeitada e não altera a exposição. Atingir exatamente R$ 1.000.000 é permitido, já que o enunciado fala em "ultrapassar".
- As validações do formulário são repetidas na API, para que não seja possível burlá-las chamando o endpoint diretamente.
- A exposição fica em memória (singleton) e é zerada quando a API reinicia. O acesso é protegido por `lock` para que requisições simultâneas não ultrapassem o limite.
- Valores monetários usam `decimal` no backend para evitar erros de arredondamento.

## Uso de IA

Este projeto foi desenvolvido com o auxílio de uma ferramenta de IA (Claude Code), usada para a escrita do código e da documentação. As decisões técnicas, a arquitetura, a escolha das tecnologias e as regras de negócio foram definidas por mim, e o código gerado foi revisado e validado antes de cada commit.
