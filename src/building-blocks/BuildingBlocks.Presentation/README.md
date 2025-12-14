# BuildingBlocks.Presentation

Componentes compartilhados para a camada de apresentação (Controllers e API).

## Estrutura

```
BuildingBlocks.Presentation/
├── Controllers/
│   └── ApiControllerBase.cs      # Base controller com helpers para Result
├── Filters/
│   ├── ExceptionHandlingFilter.cs # MVC exception filter
│   └── ValidationFilter.cs        # ModelState validation filter
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs # Global exception handler
│   └── RequestLoggingMiddleware.cs    # HTTP request/response logging
└── Extensions/
    ├── ResultExtensions.cs        # Result → IResult para Minimal APIs
    └── ProblemDetailsExtensions.cs # Error → ProblemDetails (RFC 7807)
```

## Uso

### Configuração no Program.cs

```csharp
// 1. Registrar serviços
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionHandlingFilter>();
});

// 2. Configurar pipeline (ordem importa!)
app.UseExceptionHandlingMiddleware(); // Primeiro - captura exceções
app.UseRequestLoggingMiddleware();    // Segundo - logging

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

### Controllers

```csharp
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id));
        return HandleResult(result); // Converte Result → IActionResult
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Value });
    }
}
```

### Minimal APIs

```csharp
app.MapGet("/users/{id}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetUserByIdQuery(id));
    return result.ToResult(); // Extensão de ResultExtensions
});
```

## Formato de Erro (RFC 7807)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "User with ID 123 was not found",
  "instance": "/api/users/123",
  "errorCode": "USER_NOT_FOUND",
  "traceId": "00-abc123..."
}
```

## Dependências

- Microsoft.AspNetCore.App (FrameworkReference)
- MediatR
- BuildingBlocks.Application
