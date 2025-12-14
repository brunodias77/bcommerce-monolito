# BuildingBlocks.Application

Biblioteca de blocos de construção para a **Application Layer** seguindo **CQRS** (Command Query Responsibility Segregation) com **MediatR**.

---

## 📦 Estrutura

```
BuildingBlocks.Application/
├── Abstractions/
│   ├── ICommand.cs                    # Interface para comandos (write)
│   ├── IQuery.cs                      # Interface para queries (read)
│   └── ICommandHandler.cs             # Handlers tipados
│
├── Behaviors/                         # Pipelines do MediatR
│   ├── ValidationBehavior.cs          # Validação automática (FluentValidation)
│   ├── LoggingBehavior.cs             # Logs de entrada/saída
│   └── TransactionBehavior.cs         # Transações automáticas em Commands
│
├── Pagination/
│   ├── PagedResult.cs                 # Resultado paginado
│   └── PaginationParams.cs            # Parâmetros de paginação
│
└── Results/
    ├── Result.cs                      # Result pattern aprimorado
    └── Error.cs                       # Erros tipados
```

---

## 🎯 CQRS com MediatR

### Commands (Write Operations)

Commands modificam o estado do sistema:

```csharp
// Command sem retorno de valor
public record CreateProductCommand(
    string Sku,
    string Name,
    decimal Price,
    int InitialStock
) : ICommand;

// Handler
internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = Product.Create(
            command.Sku,
            command.Name,
            command.Price,
            command.InitialStock);

        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

// Command com retorno de valor (Id)
public record CreateOrderCommand(
    Guid UserId,
    List<OrderItemDto> Items
) : ICommand<Guid>;

internal class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(command.UserId, command.Items);
        await _repository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(order.Id);
    }
}
```

### Queries (Read Operations)

Queries retornam dados **sem modificar estado**:

```csharp
// Query simples
public record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;

internal class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repository;

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(query.ProductId, ct);

        if (product == null)
            return Result.Fail<ProductDto>(Error.NotFound("PRODUCT_NOT_FOUND", "Product not found"));

        return Result.Ok(ProductDto.FromEntity(product));
    }
}

// Query com paginação
public record SearchProductsQuery(
    string? SearchTerm,
    Guid? CategoryId,
    int PageNumber,
    int PageSize
) : IQuery<PagedResult<ProductDto>>;

internal class SearchProductsQueryHandler
    : IQueryHandler<SearchProductsQuery, PagedResult<ProductDto>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(
        SearchProductsQuery query,
        CancellationToken ct)
    {
        var pagination = new PaginationParams(query.PageNumber, query.PageSize);

        var pagedResult = await _dbContext.Products
            .Where(p => p.DeletedAt == null)
            .Where(p => query.CategoryId == null || p.CategoryId == query.CategoryId)
            .ToPagedResultAsync(pagination, ct);

        var dtos = pagedResult.Map(p => ProductDto.FromEntity(p));

        return Result.Ok(dtos);
    }
}
```

---

## 🔄 MediatR Behaviors (Pipeline)

Behaviors interceptam **todos** os requests antes de chegarem ao handler.

### Ordem de Execução

```
Request → ValidationBehavior → LoggingBehavior → TransactionBehavior → Handler
```

### 1. ValidationBehavior

Valida automaticamente usando **FluentValidation**:

```csharp
// Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be positive");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .Matches("^[A-Z0-9-]+$")
            .WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0);
    }
}

// Se validação falhar, retorna:
// Result.Fail(Error.Validation("VALIDATION_ERROR", "Name: Name is required; Price: Price must be positive"))
```

### 2. LoggingBehavior

Loga automaticamente entrada, saída e tempo de execução:

```csharp
// Logs gerados:
[INFO] Handling CreateProductCommand { Sku: "PROD-001", Name: "Product 1" }
[INFO] Handled CreateProductCommand in 234ms with result: Success

[WARN] Handled CreateProductCommand in 1234ms with result: Failure - VALIDATION_ERROR
```

**Variações**:

- **LoggingBehavior**: Básico (produção)
- **DetailedLoggingBehavior**: JSON completo (desenvolvimento)
- **PerformanceLoggingBehavior**: Alerta se exceder threshold

### 3. TransactionBehavior

Abre transação **apenas em Commands**:

```csharp
// Command → Abre transação
public record CreateOrderCommand(...) : ICommand<Guid>;

// Query → NÃO abre transação (read-only)
public record GetOrderQuery(...) : IQuery<OrderDto>;
```

**Fluxo**:

1. Detecta se é Command
2. Abre transação no UnitOfWork
3. Executa handler
4. Se `Result.IsSuccess` → COMMIT
5. Se `Result.IsFailure` ou exceção → ROLLBACK

---

## ✅ Result Pattern

### Result e Result<T>

```csharp
// Sucesso sem valor
return Result.Ok();

// Sucesso com valor
return Result.Ok(order.Id);

// Falha com erro tipado
return Result.Fail(Error.NotFound("PRODUCT_NOT_FOUND", "Product not found"));

// Uso no Controller
var result = await _mediator.Send(new CreateOrderCommand(...));

if (result.IsFailure)
    return BadRequest(new { error = result.Error.Message, code = result.Error.Code });

return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, null);
```

### Error Tipados

```csharp
// Erros predefinidos por tipo
Error.Validation("INVALID_PRICE", "Price must be positive");        // 400
Error.NotFound("PRODUCT_NOT_FOUND", "Product not found");          // 404
Error.Conflict("INSUFFICIENT_STOCK", "Insufficient stock");        // 409
Error.Unauthorized("INVALID_TOKEN", "Token is invalid");           // 401
Error.Forbidden("ACCESS_DENIED", "Access denied");                 // 403
Error.Failure("PAYMENT_FAILED", "Payment gateway error");          // 500

// Extensões para erros comuns do domínio
ErrorExtensions.ProductNotFound(productId);
ErrorExtensions.InsufficientStock(available: 5, requested: 10);
ErrorExtensions.InvalidCoupon("Coupon expired");
```

### Operações com Result

```csharp
// Map: Transforma valor
Result<Order> orderResult = await GetOrder(orderId);
Result<OrderDto> dtoResult = orderResult.Map(o => OrderDto.FromEntity(o));

// Bind: Encadeia operações
Result<Payment> paymentResult = orderResult
    .Bind(order => ProcessPayment(order));

// OnSuccess / OnFailure
result
    .OnSuccess(order => _logger.LogInformation("Order created: {OrderId}", order.Id))
    .OnFailure(error => _logger.LogWarning("Failed to create order: {Error}", error.Message));

// GetValueOrThrow
var order = result.GetValueOrThrow(); // Lança exceção se falha

// TryGetValue
if (result.TryGetValue(out var order))
{
    // Usar order
}
```

---

## 📄 Paginação

### PaginationParams

```csharp
var pagination = new PaginationParams(
    pageNumber: 1,
    pageSize: 10
);

// Ou via query string
var pagination = PaginationParams.FromQuery(
    page: Request.Query["page"],
    size: Request.Query["size"]
);

// Com ordenação
var paginationWithSort = new PaginationWithSortParams(
    pageNumber: 1,
    pageSize: 10,
    sortBy: "createdAt",
    sortDirection: "desc"
);
```

### PagedResult<T>

```csharp
// Criar manualmente
var pagedResult = PagedResult<ProductDto>.Create(
    items: products,
    pageNumber: 1,
    pageSize: 10,
    totalCount: 156
);

// Via extensão (Entity Framework)
var pagedResult = await dbContext.Products
    .Where(p => p.DeletedAt == null)
    .ToPagedResultAsync(pagination, cancellationToken);

// Mapear itens
var dtoPagedResult = pagedResult.Map(p => ProductDto.FromEntity(p));

// Metadados
var metadata = pagedResult.GetMetadata();
// {
//   PageNumber: 1,
//   PageSize: 10,
//   TotalCount: 156,
//   TotalPages: 16,
//   HasPreviousPage: false,
//   HasNextPage: true
// }
```

### Uso em Controller

```csharp
[HttpGet]
public async Task<IActionResult> SearchProducts(
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int size = 10)
{
    var query = new SearchProductsQuery(search, page, size);
    var result = await _mediator.Send(query);

    if (result.IsFailure)
        return BadRequest(result.Error);

    var pagedResult = result.Value;

    // Adicionar metadados no header (padrão REST)
    Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagedResult.GetMetadata()));

    return Ok(pagedResult.Items);
}
```

---

## 🚀 Configuração no Startup

### 1. Registrar MediatR

```csharp
// Program.cs ou Startup.cs
builder.Services.AddMediatR(cfg =>
{
    // Registrar assemblies com handlers
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQueryHandler).Assembly);
});
```

### 2. Registrar Behaviors

```csharp
// Ordem importa! Do mais externo ao mais interno:
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

// Ou via extensões:
builder.Services
    .AddLoggingBehavior()
    .AddValidationBehavior()
    .AddTransactionBehavior();
```

### 3. Registrar Validators

```csharp
// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);
```

### 4. Registrar UnitOfWork

```csharp
// DbContext implementa IUnitOfWork
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
```

---

## 📊 Exemplo Completo

### Command

```csharp
// 1. Command
public record CreateProductCommand(
    string Sku,
    string Name,
    decimal Price,
    int InitialStock
) : ICommand<Guid>;

// 2. Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Sku).NotEmpty().Matches("^[A-Z0-9-]+$");
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0);
    }
}

// 3. Handler
internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        // Verificar SKU duplicado
        var existing = await _repository.GetBySkuAsync(command.Sku, ct);
        if (existing != null)
            return Result.Fail<Guid>(Error.Conflict("SKU_DUPLICATED", "SKU already exists"));

        // Criar produto
        var product = Product.Create(command.Sku, command.Name, command.Price, command.InitialStock);

        // Persistir
        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Ok(product.Id);
    }
}

// 4. Controller
[HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
{
    var command = new CreateProductCommand(request.Sku, request.Name, request.Price, request.InitialStock);
    var result = await _mediator.Send(command);

    if (result.IsFailure)
        return BadRequest(new { error = result.Error.Message, code = result.Error.Code });

    return CreatedAtAction(nameof(GetProduct), new { id = result.Value }, null);
}
```

### Query com Paginação

```csharp
// 1. Query
public record SearchProductsQuery(
    string? SearchTerm,
    Guid? CategoryId,
    int PageNumber,
    int PageSize
) : IQuery<PagedResult<ProductDto>>;

// 2. Handler
internal class SearchProductsQueryHandler
    : IQueryHandler<SearchProductsQuery, PagedResult<ProductDto>>
{
    private readonly CatalogDbContext _dbContext;

    public async Task<Result<PagedResult<ProductDto>>> Handle(
        SearchProductsQuery query,
        CancellationToken ct)
    {
        var pagination = new PaginationParams(query.PageNumber, query.PageSize);

        var productsQuery = _dbContext.Products
            .Where(p => p.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            productsQuery = productsQuery.Where(p => p.Name.Contains(query.SearchTerm));

        if (query.CategoryId.HasValue)
            productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId);

        var pagedResult = await productsQuery
            .OrderBy(p => p.Name)
            .ToPagedResultAsync(pagination, ct);

        var dtos = pagedResult.Map(p => ProductDto.FromEntity(p));

        return Result.Ok(dtos);
    }
}

// 3. Controller
[HttpGet]
public async Task<IActionResult> SearchProducts(
    [FromQuery] string? search,
    [FromQuery] Guid? categoryId,
    [FromQuery] int page = 1,
    [FromQuery] int size = 10)
{
    var query = new SearchProductsQuery(search, categoryId, page, size);
    var result = await _mediator.Send(query);

    if (result.IsFailure)
        return BadRequest(result.Error);

    var pagedResult = result.Value;
    Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagedResult.GetMetadata()));

    return Ok(pagedResult.Items);
}
```

---

## 🎓 Boas Práticas

### ✅ DO:

- Use Commands para operações que modificam estado
- Use Queries para operações de leitura
- Sempre retorne `Result` ou `Result<T>`
- Implemente validators para todos os Commands
- Use DTOs para retorno de Queries (nunca retorne entidades)
- Mantenha handlers pequenos e focados (SRP)
- Use paginação em queries que retornam listas

### ❌ DON'T:

- Não modifique estado em Queries
- Não retorne entidades de domínio em Queries
- Não misture lógica de domínio em handlers
- Não use exceções para controle de fluxo
- Não faça queries dentro de loops

---

## 📖 Referências

- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Result Pattern](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)

---

**Versão**: 1.0.0  
**Data**: 2025-12-13  
**Projeto**: E-commerce Modular Monolith
