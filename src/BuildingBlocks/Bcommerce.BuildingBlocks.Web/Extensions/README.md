# Web Extensions

Este diretório centraliza a configuração do pipeline HTTP e a injeção de dependência para a camada Web (API).

## `ApplicationBuilderExtensions.cs`
**Responsabilidade:** Configurar a ordem de execução dos Middlewares no pipeline de requisição do ASP.NET Core.
**Por que existe:** A ordem dos middlewares é crucial e crítica. Centralizar aqui garante que todas as APIs sigam a mesma sequência (ex: ExceptionHandler -> CorrelationId -> Logging -> Auth).
**Em que situação usar:** No `Program.cs` após o `Build()`.
**O que pode dar errado:** 
- **Ordem Incorreta:** Se alguém mexer na ordem interna, pode quebrar funcionalidades. Por exemplo, se o `ExceptionHandlingMiddleware` não for o primeiro, exceções lançadas por middlewares anteriores não serão capturadas por ele.
**Exemplo real de uso:**
```csharp
var app = builder.Build();
// Configura todo o pipeline base (Logging, Error handling, etc)
app.UseBuildingBlocksWeb();
app.MapControllers();
```

---

## `ServiceCollectionExtensions.cs`
**Responsabilidade:** Registrar controllers, filtros globais e serviços de middleware no container de DI.
**Por que existe:** Para encapsular configurações opinativas do framework, como a supressão do filtro padrão de validação do .NET (`SuppressModelStateInvalidFilter = true`), permitindo que usemos nossa própria padronização de erros de validação via `ValidationFilter`.
**Em que situação usar:** No `Program.cs` antes do `Build()`.
**O que pode dar errado:** 
- Ao suprimir o `ModelStateInvalidFilter`, os controllers não retornarão mais 400 Bad Request automaticamente se o model for inválido. É **obrigatório** que o `ValidationFilter` (registrado aqui) esteja funcionando corretamente, caso contrário, requests inválidos entrarão nos Actions.
**Exemplo real de uso:**
```csharp
builder.Services.AddBuildingBlocksWeb();
```
