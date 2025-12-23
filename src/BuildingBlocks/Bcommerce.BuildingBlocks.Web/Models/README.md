# Web Models

Este diretório contém os modelos que definem o contrato de resposta padrão da API (Data Contracts / DTOs de Resposta).

## `ApiResponse.cs`
**Responsabilidade:** Ser o envelope (wrapper) padrão para todas as respostas da API, unificando o formato de sucesso e falha.
**Por que existe:** Para que o frontend/consumidor não precise adivinhar a estrutura da resposta. Ele sempre receberá `{ success: boolean, data: ..., error: ... }`.
**Em que situação usar:** Em todos os retornos de Controllers, especialmente aqueles que retornam dados.
**O que pode dar errado:** Se usado incorretamente misturando dados de sucesso com erro (o construtor privado impede isso, mas factories manuais mal feitas poderiam tentar).
**Exemplo real de uso:**
```csharp
[HttpGet]
public IActionResult Get() => Ok(ApiResponse<string>.Ok("Hello"));
```

---

## `ErrorResponse.cs`
**Responsabilidade:** Padronizar o objeto JSON retornado quando ocorre um erro.
**Por que existe:** Para evitar inconsistências onde um endpoint retorna `{ message: "erro" }` e outro retorna `{ errors: ["erro"] }`.
**Em que situação usar:** Usado internamente pelo `ApiResponse` ou diretamente por Middlewares de exceção.
**Exemplo real de uso:**
```json
{
  "code": "Order.NotFound",
  "message": "Pedido 123 não encontrado",
  "type": "NotFound"
}
```

---

## `ValidationErrorResponse.cs`
**Responsabilidade:** Especialização de `ErrorResponse` para transportar múltiplos erros de validação (campo -> mensagens).
**Por que existe:** Um erro simples (string) não é suficiente para explicar erros em formulários com múltiplos campos inválidos.
**Em que situação usar:** Retornado automaticamente pelo `ValidationFilter` quando o `ModelState` é inválido.
**Exemplo real de uso:**
```json
{
  "code": "Validation.Error",
  "message": "Erros de validação encontrados",
  "errors": {
    "Email": ["Formato inválido"],
    "Age": ["Deve ser maior que 18"]
  }
}
```

---

## `PaginatedResponse.cs`
**Responsabilidade:** Wrapper para listas paginadas, adicionando metadados de navegação.
**Por que existe:** Retornar apenas um array JSON `[]` perde informações cruciais como "qual página é essa?" e "quantas páginas existem?".
**Em que situação usar:** Em endpoints de listagem que suportam paginação.
**O que pode dar errado:** Retornar uma lista gigante sem paginação causa problemas de performance. Este modelo *obriga* o uso de uma estrutura paginada.
**Exemplo real de uso:**
```csharp
// Response JSON:
{
  "items": [...],
  "pageNumber": 1,
  "totalPages": 5,
  "totalCount": 50
}
```
