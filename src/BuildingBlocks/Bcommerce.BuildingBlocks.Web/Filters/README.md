# Web Filters

Este diretório contém filtros de ação e exceção para interceptar e padronizar o comportamento das APIs.

## `ApiExceptionFilter.cs`
**Responsabilidade:** Ser a última linha de defesa para erros não tratados na API. Ele captura exceções lançadas nos Controllers ou Services e as converte em respostas JSON (ProblemDetails/ErrorResponse) com Status Code apropriado.
**Por que existe:** Para garantir que o cliente (Front-end/App) receba sempre um JSON estruturado em caso de erro, e não uma stack trace HTML ou um "Internal Server Error" vazio.
**Em que situação usar:** Configurado globalmente nas opções do MVC/Controllers.
**O que pode dar errado:** Se for removido, erros de validação (`ValidationException`) ou lógica (`DomainException`) retornarão 500 para o cliente, dificultando o debug e tratamento no front-end.
**Exemplo real de uso:**
```csharp
// Converte:
throw new NotFoundException("Pedido não encontrado");
// Em HTTP 404:
// { "code": "Resource.NotFound", "message": "Pedido não encontrado" }
```

---

## `ValidationFilter.cs`
**Responsabilidade:** Interceptar requisições onde o `ModelState` é inválido (ex: campos obrigatórios faltando no JSON) ANTES de entrar na Action do Controller.
**Por que existe:** O ASP.NET Core tem um filtro padrão, mas este filtro customizado permite retornar o nosso formato `ValidationErrorResponse` em vez do formato padrão do framework.
**Em que situação usar:** Em conjunto com `SuppressModelStateInvalidFilter = true` no startup.
**O que pode dar errado:** Se a supressão do filtro padrão não for configurada, este filtro pode rodar duplicado ou nem rodar (se o padrão rodar antes e curto-circuitar a request).
**Exemplo real de uso:**
```csharp
// Payload inválido -> Retorna 400 Bad Request
{
  "code": "Validation.Error",
  "errors": { "Email": ["Inválido"] }
}
```

---

## `AuthorizationFilter.cs`
**Responsabilidade:** Ponto de extensão para lógicas de autorização que fogem do padrão de Policies do ASP.NET Core (ex: validar um header customizado de API Key).
**Por que existe:** Atualmente serve como um placeholder/exemplo. A autorização principal deve ser feita via Policies (`[Authorize]`).
**Em que situação usar:** Apenas se houver requisitos muito específicos de segurança que não podem ser atendidos por Policies ou Middlewares.
**Exemplo real de uso:**
```csharp
// Se precisasse validar um header X-Special-Token manualmente
if (!context.HttpContext.Request.Headers.ContainsKey("X-Special-Token")) {
    context.Result = new UnauthorizedResult();
}
```
