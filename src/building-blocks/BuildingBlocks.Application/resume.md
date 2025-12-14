# BuildingBlocks.Application - Implementação Completa

## 📦 Resumo da Entrega

Implementação completa da **Application Layer** com **CQRS**, **MediatR**, **Behaviors** e **Result Pattern** para o sistema de e-commerce modular monolith.

---

## 📂 Estrutura Entregue

```
BuildingBlocks.Application/
│
├── 📁 Abstractions/                       # Interfaces CQRS
│   ├── ICommand.cs                        # ✅ Interface para comandos (write)
│   ├── IQuery.cs                          # ✅ Interface para queries (read)
│   └── ICommandHandler.cs                 # ✅ Handlers tipados (Command + Query)
│
├── 📁 Behaviors/                          # Pipelines do MediatR
│   ├── ValidationBehavior.cs              # ✅ Validação automática (FluentValidation)
│   ├── LoggingBehavior.cs                 # ✅ Logs automáticos + Performance
│   └── TransactionBehavior.cs             # ✅ Transações automáticas em Commands
│
├── 📁 Pagination/                         # Sistema de paginação
│   ├── PagedResult.cs                     # ✅ Resultado paginado tipado
│   └── PaginationParams.cs                # ✅ Parâmetros + Extensões
│
├── 📁 Results/                            # Result Pattern aprimorado
│   ├── Error.cs                           # ✅ Erros tipados por categoria
│   └── Result.cs                          # ✅ Result + Result<T>
│
├── 📄 BuildingBlocks.Application.csproj   # ✅ Projeto .NET 8
├── 📄 .editorconfig                       # ✅ Configurações de código
├── 📄 README.md                           # ✅ Documentação completa
└── 📄 EXAMPLES.md                         # ✅ 7 categorias de exemplos
```

---

## ✨ Destaques da Implementação

### 1. **CQRS Completo**

Separação clara entre Commands (escrita) e Queries (leitura):

```csharp
// Command → Modifica estado
public record CreateProductCommand(...) : ICommand<Guid>;

// Query → Apenas leitura
public record GetProductByIdQuery(...) : IQuery<ProductDto>;
```

### 2. **MediatR Behaviors (Pipeline)**

3 Behaviors essenciais implementados:

- **ValidationBehavior**: Valida automaticamente com FluentValidation
- **LoggingBehavior**: Loga entrada, saída, duração e erros
- **TransactionBehavior**: Abre transação apenas em Commands

**Pipeline de execução:**

```
Request → Logging → Validation → Transaction → Handler
```

### 3. **Result Pattern Aprimorado**

```csharp
// Erros tipados por categoria
Error.Validation("INVALID_PRICE", "Price must be positive");    // 400
Error.NotFound("PRODUCT_NOT_FOUND", "Product not found");       // 404
Error.Conflict("SKU_DUPLICATED", "SKU already exists");         // 409

// Result<T> com operações funcionais
result
    .Map(order => OrderDto.FromEntity(order))
    .Bind(dto => EnrichWithPaymentInfo(dto))
    .OnSuccess(dto => _logger.LogInfo("Order created"))
    .OnFailure(error => _logger.LogWarning(error.Message));
```

### 4. **Paginação Robusta**

```csharp
var pagination = new PaginationParams(page: 1, size: 10);

var pagedResult = await dbContext.Products
    .Where(p => p.DeletedAt == null)
    .ToPagedResultAsync(pagination, ct);

// PagedResult contém:
// - Items (lista da página)
// - PageNumber, PageSize, TotalCount, TotalPages
// - HasNextPage, HasPreviousPage
// - GetMetadata() para APIs REST
```

### 5. **Extensões para Controllers**

Helper para mapear `Error` → HTTP Status Code:

```csharp
private IActionResult HandleFailure(Error error)
{
    return error.Type switch
    {
        ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
        ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
        ErrorType.Conflict => Conflict(new { error.Code, error.Message }),
        _ => StatusCode(500, new { error.Code, error.Message })
    };
}
```

---

## 🎯 Exemplos Práticos Incluídos

### 1. Commands Completos

- ✅ CreateProductCommand (com validação complexa)
- ✅ CreateOrderCommand (comunicação entre módulos)
- ✅ UpdateProductPriceCommand (regras de negócio)

### 2. Queries com Paginação

- ✅ SearchProductsQuery (filtros + ordenação + paginação)
- ✅ GetUserOrdersQuery (filtros opcionais)

### 3. Validators FluentValidation

- ✅ Validações síncronas e assíncronas
- ✅ Regras condicionais (When)
- ✅ Validações customizadas (MustAsync)

### 4. Behaviors em Ação

- ✅ Pipeline completo configurado
- ✅ Logs detalhados gerados
- ✅ Métricas de performance

### 5. Result Pattern Avançado

- ✅ Encadeamento com Bind e Map
- ✅ Combinação de múltiplos Results
- ✅ Tratamento de erros agregados

### 6. Controllers ASP.NET Core

- ✅ Controller completo (CRUD)
- ✅ Mapeamento de erros para HTTP Status
- ✅ Metadados de paginação em headers

### 7. Testes Unitários

- ✅ Testes de handlers com mocks
- ✅ Testes de validação
- ✅ Testes de cenários de erro

---

## 🔗 Integração com BuildingBlocks.Domain

```csharp
// BuildingBlocks.Application.csproj
<ItemGroup>
  <ProjectReference Include="..\BuildingBlocks.Domain\BuildingBlocks.Domain.csproj" />
</ItemGroup>

// Uso conjunto
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Results;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository; // Domain
    private readonly IUnitOfWork _unitOfWork;        // Domain

    public async Task<Result<Guid>> Handle(...)      // Application
    {
        var product = Product.Create(...);           // Domain
        await _repository.AddAsync(product);         // Domain
        return Result.Ok(product.Id);                // Application
    }
}
```

---

## 🚀 Como Usar

### 1. Registrar no Startup

```csharp
// Program.cs
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
});

// Behaviors (ordem importa!)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
```

### 2. Criar Command/Query

```csharp
// Command
public record CreateProductCommand(
    string Sku,
    string Name,
    decimal Price
) : ICommand<Guid>;

// Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// Handler
internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = Product.Create(command.Sku, command.Name, command.Price, 0);
        await _repository.AddAsync(product, ct);
        return Result.Ok(product.Id);
    }
}
```

### 3. Usar no Controller

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
{
    var command = new CreateProductCommand(request.Sku, request.Name, request.Price);
    var result = await _mediator.Send(command);

    if (result.IsFailure)
        return HandleFailure(result.Error);

    return CreatedAtAction(nameof(GetById), new { id = result.Value }, null);
}
```

---

## 📊 Fluxo Completo

```
HTTP Request
    ↓
Controller (cria Command)
    ↓
MediatR.Send(command)
    ↓
LoggingBehavior (loga entrada)
    ↓
ValidationBehavior (FluentValidation)
    ↓
TransactionBehavior (abre transação)
    ↓
CommandHandler (lógica de negócio)
    ↓
Repository (acessa dados)
    ↓
UnitOfWork.SaveChanges() (persiste)
    ↓
TransactionBehavior (commit/rollback)
    ↓
LoggingBehavior (loga saída + duração)
    ↓
Result<T> retornado
    ↓
Controller (mapeia para HTTP Status)
    ↓
HTTP Response
```

---

## 📚 Recursos Disponíveis

### Documentação

- **README.md**: 500+ linhas com conceitos, exemplos e configuração
- **EXAMPLES.md**: 7 categorias com 15+ exemplos completos

### Dependências

- **MediatR 12.2.0**: Mediator pattern
- **FluentValidation 11.9.0**: Validações declarativas
- **Microsoft.Extensions.Logging**: Logging abstraction
- **Microsoft.Extensions.DependencyInjection**: DI abstraction

---

## ✅ Checklist de Qualidade

- [x] CQRS completo (Commands + Queries)
- [x] MediatR configurado e documentado
- [x] ValidationBehavior com FluentValidation
- [x] LoggingBehavior (básico + detalhado + performance)
- [x] TransactionBehavior (apenas Commands)
- [x] Result Pattern tipado (Error + Result<T>)
- [x] Paginação completa (PagedResult + PaginationParams)
- [x] Extensões para Controllers
- [x] Integração com BuildingBlocks.Domain
- [x] Exemplos práticos baseados no e-commerce
- [x] Documentação completa
- [x] EditorConfig configurado
- [x] .NET 8 ready

---

## 🎓 Conceitos Implementados

1. **CQRS**: Separação Commands/Queries
2. **MediatR**: Mediator pattern
3. **Pipeline Behaviors**: Validação, Logging, Transação
4. **Result Pattern**: Tratamento de erros sem exceções
5. **FluentValidation**: Validações declarativas
6. **Paginação**: PagedResult + metadados
7. **Error Handling**: Erros tipados por categoria
8. **Functional Programming**: Map, Bind, OnSuccess, OnFailure
9. **Unit of Work**: Transações automáticas
10. **Separation of Concerns**: Application ≠ Domain

---

## 📖 Próximos Passos

1. **Implementar módulos específicos**:

   - Catalog.Application (Commands/Queries de produtos)
   - Orders.Application (Commands/Queries de pedidos)
   - Payments.Application (Commands/Queries de pagamentos)

2. **Adicionar Behaviors extras**:

   - CachingBehavior (cache de queries)
   - AuthorizationBehavior (validação de permissões)
   - RetryBehavior (retry automático em falhas)

3. **Implementar testes**:
   - Testes de integração com TestContainers
   - Testes de behaviors
   - Testes de validators

---

**Projeto**: E-commerce Modular Monolith  
**Versão**: 1.0.0  
**Data**: 2025-12-13  
**Status**: ✅ Implementação Completa

---

## 💡 Arquitetura Final

```
BuildingBlocks/
├── Domain/                 ← Entidades, Value Objects, Events
│   └── (já implementado)
│
└── Application/            ← CQRS, Behaviors, Result Pattern
    └── (implementado agora)

Próximos:
├── Infrastructure/         ← EF Core, Repositories, Event Bus
├── Presentation/           ← Controllers, DTOs, Mappers
└── Tests/                  ← Unit, Integration, E2E
```

**Happy Coding! 🚀**
