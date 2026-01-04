# Padrão de Implementação: Command e Controller

Este documento serve como guia de implementação para garantir a consistência no uso de CQRS, Result Pattern, Tratamento de Erros e Respostas de API.

## 1. Estrutura do Command (Application Layer)

O Command deve representar a intenção do usuário. Utilizamos `Result<T>` para evitar lançar exceções em fluxos de negócio esperados (ex: e-mail duplicado, produto sem estoque).

### 1.1 Command Definition
```csharp
// Modules/Catalog/Application/Products/CreateProduct/CreateProductCommand.cs
public record CreateProductCommand(
    string Name, 
    decimal Price, 
    int Stock, 
    Guid CategoryId
) : ICommand<Result<Guid>>; // Retorna o ID do produto criado
```

### 1.2 Command Validator (FluentValidation)
A validação ocorre **antes** do Handler, graças ao `ValidationBehavior` configurado no BuildingBlocks.

```csharp
// Modules/Catalog/Application/Products/CreateProduct/CreateProductCommandValidator.cs
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
```

### 1.3 Command Handler
O Handler foca na regra de negócio. Ele não se preocupa com HTTP Status Codes.

```csharp
// Modules/Catalog/Application/Products/CreateProduct/CreateProductCommandHandler.cs
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork; // Opcional se usar TransactionBehavior

    public CreateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        // 1. Verificar regras de domínio que dependem do banco
        if (await _repository.ExistsByNameAsync(command.Name, cancellationToken))
        {
            // Retorna erro de domínio (não lança exceção)
            return Result.Failure<Guid>(DomainErrors.Product.NameAlreadyExists);
        }

        // 2. Criar Entidade
        var product = Product.Create(
            command.Name, 
            command.Price, 
            command.Stock, 
            command.CategoryId
        );

        // 3. Persistir
        await _repository.AddAsync(product, cancellationToken);
        
        // 4. Retornar Sucesso
        return Result.Success(product.Id);
    }
}
```

---

## 2. Estrutura do Controller (Presentation Layer)

O Controller deve ser "burro". Ele apenas recebe a requisição, despacha para o MediatR e converte o `Result` para uma resposta HTTP (200, 400, 404).

Utilizamos o `ApiControllerBase` do BuildingBlocks para padronizar essa conversão.

### 2.1 ApiControllerBase (BuildingBlocks.Web)
Esta classe base facilita o retorno de respostas baseadas no `Result`.

```csharp
// BuildingBlocks.Web/Controllers/ApiControllerBase.cs
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected readonly ISender Sender;

    protected ApiControllerBase(ISender sender)
    {
        Sender = sender;
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is null ? NoContent() : Ok(result.Value);
        }

        return HandleFailure(result.Error);
    }

    protected IActionResult HandleFailure(Error error)
    {
        // Mapeia tipos de erro para Status Codes
        return error.Type switch
        {
            ErrorType.Validation => BadRequest(CreateProblemDetails("Validation Error", error)),
            ErrorType.NotFound => NotFound(CreateProblemDetails("Not Found", error)),
            ErrorType.Conflict => Conflict(CreateProblemDetails("Conflict", error)),
            _ => StatusCode(500, CreateProblemDetails("Internal Error", error))
        };
    }
    
    private ProblemDetails CreateProblemDetails(string title, Error error)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = error.Description,
            Status = GetStatusCode(error.Type),
            Type = error.Code
        };
    }
}
```

### 2.2 ProductsController (Implementation)

Este controller demonstra o uso da herança do `ApiControllerBase` definido no **BuildingBlocks.Web**.

```csharp
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BuildingBlocks.Web.Controllers; // Namespace do Building Blocks
using Catalog.Application.Products.CreateProduct;
using Catalog.Application.Products.GetProductById;

namespace Catalog.Presentation.Controllers;

public class ProductsController : ApiControllerBase
{
    public ProductsController(ISender sender) : base(sender)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        // 1. Converter Request (DTO) para Command
        var command = new CreateProductCommand(
            request.Name, 
            request.Price, 
            request.Stock, 
            request.CategoryId
        );

        // 2. Enviar para o MediatR
        var result = await Sender.Send(command);

        // 3. Retornar Resultado Padronizado usando método da base
        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return CreatedAtAction(
            nameof(GetById), 
            new { id = result.Value }, 
            result.Value
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Sender.Send(query);
        
        // Usa o método da base para converter Result<T> em 200 OK ou 404/400
        return HandleResult(result);
    }
}
```

---

## 3. Tratamento de Exceções (GlobalExceptionHandler)

Exceções não tratadas (bugs, falhas de banco de dados) são capturadas pelo Middleware global e convertidas para o padrão **RFC 7807 (Problem Details)**.

```csharp
// BuildingBlocks.Web/Middlewares/GlobalExceptionHandler.cs
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred."
            // Não expor stack trace em produção!
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

---

## Resumo das Regras

1.  **Commands/Queries** retornam `Result<T>` e não lançam exceções de negócio.
2.  **Handlers** focam apenas no domínio e persistência.
3.  **Controllers** herdam de `ApiControllerBase` e apenas despacham comandos.
4.  **Validações** de entrada (formato, tamanho) são feitas via FluentValidation.
5.  **Exceções inesperadas** são tratadas globalmente pelo Middleware.
