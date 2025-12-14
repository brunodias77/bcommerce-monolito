# Exemplos Práticos - BuildingBlocks.Application

Exemplos completos baseados no sistema de e-commerce modular monolith.

---

## 📋 Índice

1. [Commands Completos](#1-commands-completos)
2. [Queries com Paginação](#2-queries-com-paginação)
3. [Validators FluentValidation](#3-validators-fluentvalidation)
4. [Behaviors em Ação](#4-behaviors-em-ação)
5. [Result Pattern Avançado](#5-result-pattern-avançado)
6. [Controllers ASP.NET Core](#6-controllers-aspnet-core)
7. [Testes Unitários](#7-testes-unitários)

---

## 1. Commands Completos

### Exemplo 1: CreateProductCommand (Catalog Module)

```csharp
// Command
public record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    int InitialStock
) : ICommand<Guid>;

// Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[A-Z0-9-]+$")
            .WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero");

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial stock cannot be negative");
    }
}

// Handler
internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        // Validar categoria existe
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category == null)
            return Result.Fail<Guid>(Error.NotFound("CATEGORY_NOT_FOUND", "Category not found"));

        // Validar SKU único
        var existingProduct = await _productRepository.GetBySkuAsync(command.Sku, cancellationToken);
        if (existingProduct != null)
            return Result.Fail<Guid>(Error.Conflict("SKU_DUPLICATED", $"Product with SKU '{command.Sku}' already exists"));

        // Criar produto
        var product = Product.Create(
            sku: command.Sku,
            name: command.Name,
            price: command.Price,
            initialStock: command.InitialStock);

        product.UpdateDescription(command.Description ?? string.Empty);
        product.AssignCategory(command.CategoryId);

        // Persistir
        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Product created with ID {ProductId} and SKU {Sku}",
            product.Id,
            product.Sku);

        return Result.Ok(product.Id);
    }
}
```

### Exemplo 2: CreateOrderCommand (Orders Module)

```csharp
// DTOs
public record OrderItemDto(Guid ProductId, int Quantity);
public record CreateOrderRequest(
    List<OrderItemDto> Items,
    Guid ShippingAddressId,
    string? CouponCode
);

// Command
public record CreateOrderCommand(
    Guid UserId,
    List<OrderItemDto> Items,
    Guid ShippingAddressId,
    string? CouponCode
) : ICommand<Guid>;

// Validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.Quantity).GreaterThan(0);
            });

        RuleFor(x => x.ShippingAddressId)
            .NotEmpty();

        RuleFor(x => x.CouponCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.CouponCode));
    }
}

// Handler
internal class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<Guid>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Buscar endereço via Query (comunicação com módulo Users)
        var addressQuery = new GetUserAddressQuery(command.UserId, command.ShippingAddressId);
        var addressResult = await _mediator.Send(addressQuery, cancellationToken);

        if (addressResult.IsFailure)
            return Result.Fail<Guid>(addressResult.Error);

        // 2. Validar produtos e estoque
        var orderItems = new List<OrderItem>();

        foreach (var itemDto in command.Items)
        {
            // Query ao módulo Catalog
            var productQuery = new GetProductByIdQuery(itemDto.ProductId);
            var productResult = await _mediator.Send(productQuery, cancellationToken);

            if (productResult.IsFailure)
                return Result.Fail<Guid>(productResult.Error);

            var product = productResult.Value;

            // Reservar estoque via Command
            var reserveCommand = new ReserveStockCommand(itemDto.ProductId, itemDto.Quantity);
            var reserveResult = await _mediator.Send(reserveCommand, cancellationToken);

            if (reserveResult.IsFailure)
                return Result.Fail<Guid>(reserveResult.Error);

            orderItems.Add(OrderItem.Create(
                productId: product.Id,
                productSnapshot: new { product.Name, product.Sku, product.Price },
                quantity: itemDto.Quantity,
                unitPrice: product.Price));
        }

        // 3. Validar e aplicar cupom (se fornecido)
        decimal discountAmount = 0;
        if (!string.IsNullOrWhiteSpace(command.CouponCode))
        {
            var validateCouponCommand = new ValidateCouponCommand(
                command.CouponCode,
                command.UserId,
                orderItems.Sum(i => i.Subtotal));

            var couponResult = await _mediator.Send(validateCouponCommand, cancellationToken);

            if (couponResult.IsSuccess)
                discountAmount = couponResult.Value.DiscountAmount;
        }

        // 4. Criar pedido
        var order = Order.Create(
            userId: command.UserId,
            items: orderItems,
            shippingAddress: addressResult.Value,
            shippingAmount: 15.00m); // Simplificado - calcular frete real

        if (discountAmount > 0)
            order.ApplyDiscount(discountAmount);

        // 5. Persistir
        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(order.Id);
    }
}
```

---

## 2. Queries com Paginação

### Exemplo 1: SearchProductsQuery

```csharp
// Query
public record SearchProductsQuery(
    string? SearchTerm,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? InStock,
    int PageNumber,
    int PageSize,
    string? SortBy,
    string? SortDirection
) : IQuery<PagedResult<ProductDto>>;

// Handler
internal class SearchProductsQueryHandler
    : IQueryHandler<SearchProductsQuery, PagedResult<ProductDto>>
{
    private readonly CatalogDbContext _dbContext;

    public async Task<Result<PagedResult<ProductDto>>> Handle(
        SearchProductsQuery query,
        CancellationToken cancellationToken)
    {
        var productsQuery = _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.DeletedAt == null && p.Status == ProductStatus.Active)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(query.SearchTerm) ||
                p.Description.Contains(query.SearchTerm) ||
                p.Sku.Contains(query.SearchTerm));
        }

        if (query.CategoryId.HasValue)
            productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId);

        if (query.MinPrice.HasValue)
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);

        if (query.InStock == true)
            productsQuery = productsQuery.Where(p => p.Stock > 0);

        // Aplicar ordenação
        productsQuery = ApplySorting(productsQuery, query.SortBy, query.SortDirection);

        // Paginação
        var pagination = new PaginationParams(query.PageNumber, query.PageSize);
        var pagedResult = await productsQuery.ToPagedResultAsync(pagination, cancellationToken);

        // Mapear para DTOs
        var dtos = pagedResult.Map(p => new ProductDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock,
            CategoryName = p.Category?.Name,
            ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.Url
        });

        return Result.Ok(dtos);
    }

    private IQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        string? sortBy,
        string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };
    }
}
```

### Exemplo 2: GetUserOrdersQuery

```csharp
// Query
public record GetUserOrdersQuery(
    Guid UserId,
    OrderStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int PageNumber,
    int PageSize
) : IQuery<PagedResult<OrderSummaryDto>>;

// DTO
public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal Total,
    int ItemCount,
    DateTime CreatedAt
);

// Handler
internal class GetUserOrdersQueryHandler
    : IQueryHandler<GetUserOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly OrdersDbContext _dbContext;

    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(
        GetUserOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == query.UserId)
            .AsQueryable();

        // Filtros opcionais
        if (query.Status.HasValue)
            ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);

        if (query.FromDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.CreatedAt <= query.ToDate.Value);

        // Ordenação (mais recente primeiro)
        ordersQuery = ordersQuery.OrderByDescending(o => o.CreatedAt);

        // Paginação
        var pagination = new PaginationParams(query.PageNumber, query.PageSize);
        var pagedResult = await ordersQuery.ToPagedResultAsync(pagination, cancellationToken);

        // Mapear
        var dtos = pagedResult.Map(o => new OrderSummaryDto(
            Id: o.Id,
            OrderNumber: o.OrderNumber,
            Status: o.Status,
            Total: o.Total,
            ItemCount: o.Items.Count,
            CreatedAt: o.CreatedAt
        ));

        return Result.Ok(dtos);
    }
}
```

---

## 3. Validators FluentValidation

### Exemplo 1: Validator Complexo com Regras Customizadas

```csharp
public class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductPriceCommandValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.NewPrice)
            .GreaterThan(0)
            .WithMessage("Price must be positive");

        RuleFor(x => x.NewPrice)
            .MustAsync(BeReasonablePrice)
            .WithMessage("Price change is too drastic (>100%)");

        RuleFor(x => x)
            .MustAsync(HaveSufficientPermissions)
            .WithMessage("You don't have permission to change prices");
    }

    private async Task<bool> BeReasonablePrice(
        UpdateProductPriceCommand command,
        decimal newPrice,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (product == null)
            return true; // Será validado em outro lugar

        var priceChangePercent = Math.Abs((newPrice - product.Price) / product.Price * 100);

        return priceChangePercent <= 100; // Máximo 100% de variação
    }

    private Task<bool> HaveSufficientPermissions(
        UpdateProductPriceCommand command,
        ValidationContext<UpdateProductPriceCommand> context,
        CancellationToken cancellationToken)
    {
        // Verificar permissões do usuário
        // var currentUser = _httpContextAccessor.HttpContext?.User;
        // return currentUser.IsInRole("PriceManager");

        return Task.FromResult(true);
    }
}
```

### Exemplo 2: Validator com Regras Condicionais

```csharp
public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(3, 50)
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Code must contain only uppercase letters and numbers");

        RuleFor(x => x.DiscountType)
            .IsInEnum();

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0);

        // Validação condicional por tipo
        When(x => x.DiscountType == CouponType.Percentage, () =>
        {
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                .WithMessage("Percentage discount cannot exceed 100%");
        });

        When(x => x.DiscountType == CouponType.FixedAmount, () =>
        {
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(1000)
                .WithMessage("Fixed discount cannot exceed R$ 1000");
        });

        RuleFor(x => x.ValidFrom)
            .NotEmpty();

        RuleFor(x => x.ValidUntil)
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.MaxUses)
            .GreaterThan(0)
            .When(x => x.MaxUses.HasValue);

        RuleFor(x => x.MinPurchaseAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPurchaseAmount.HasValue);
    }
}
```

---

## 4. Behaviors em Ação

### Pipeline Completo

```csharp
// Configuração no Program.cs
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
});

// Ordem dos Behaviors (do externo ao interno)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceLoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);

// Fluxo de execução:
// Request
//   → LoggingBehavior (loga entrada)
//     → PerformanceLoggingBehavior (inicia timer)
//       → ValidationBehavior (valida com FluentValidation)
//         → TransactionBehavior (abre transação)
//           → Handler (executa lógica)
//         ← TransactionBehavior (commit/rollback)
//       ← ValidationBehavior
//     ← PerformanceLoggingBehavior (verifica tempo)
//   ← LoggingBehavior (loga saída)
// Response
```

### Logs Gerados

```
[INFO] Handling CreateProductCommand { Sku: "PROD-001", Name: "Product 1", Price: 99.90 }
[DEBUG] Beginning transaction for CreateProductCommand
[INFO] Handled CreateProductCommand in 187ms with result: Success
[DEBUG] Transaction committed for CreateProductCommand

// Se validação falhar:
[INFO] Handling CreateProductCommand { Sku: "", Name: "Product 1", Price: -10 }
[WARN] Handled CreateProductCommand in 12ms with result: Failure - VALIDATION_ERROR: Sku: SKU is required; Price: Price must be positive

// Se exceder threshold de performance:
[WARN] SLOW REQUEST: CreateOrderCommand took 1234ms (threshold: 500ms) { UserId: "...", Items: [...] }
```

---

## 5. Result Pattern Avançado

### Encadeamento de Operações

```csharp
public async Task<Result<OrderDto>> CreateAndProcessOrder(CreateOrderCommand command)
{
    // Encadeamento com Bind
    var result = await CreateOrder(command)
        .Bind(orderId => ProcessPayment(orderId))
        .Bind(paymentId => ConfirmOrder(paymentId))
        .Map(orderId => GetOrderDto(orderId));

    return result;
}

private async Task<Result<Guid>> CreateOrder(CreateOrderCommand command)
{
    // ...
    return Result.Ok(order.Id);
}

private async Task<Result<Guid>> ProcessPayment(Guid orderId)
{
    // ...
    if (paymentFailed)
        return Result.Fail<Guid>(Error.Failure("PAYMENT_FAILED", "Payment was declined"));

    return Result.Ok(payment.Id);
}

private async Task<Result<Guid>> ConfirmOrder(Guid paymentId)
{
    // ...
    return Result.Ok(order.Id);
}
```

### Tratamento de Múltiplos Results

```csharp
public async Task<Result> ProcessOrderBatch(List<Guid> orderIds)
{
    var results = new List<Result>();

    foreach (var orderId in orderIds)
    {
        var result = await ProcessOrder(orderId);
        results.Add(result);
    }

    // Combina: falha se QUALQUER um falhar
    var combinedResult = Result.Combine(results.ToArray());

    return combinedResult;
}

// Ou processar todos e coletar erros
public async Task<Result> ProcessOrderBatchWithErrors(List<Guid> orderIds)
{
    var errors = new List<Error>();

    foreach (var orderId in orderIds)
    {
        var result = await ProcessOrder(orderId);

        if (result.IsFailure)
            errors.Add(result.Error);
    }

    if (errors.Any())
    {
        var aggregatedError = Error.Failure(
            "BATCH_PROCESSING_FAILED",
            $"{errors.Count} orders failed: {string.Join(", ", errors.Select(e => e.Code))}");

        return Result.Fail(aggregatedError);
    }

    return Result.Ok();
}
```

---

## 6. Controllers ASP.NET Core

### Controller Completo

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var query = new SearchProductsQuery(search, categoryId, null, null, null, page, size, null, null);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return HandleFailure(result.Error);

        var pagedResult = result.Value;

        // Adicionar metadados de paginação no header
        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagedResult.GetMetadata()));

        return Ok(pagedResult.Items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return HandleFailure(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.Sku,
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.InitialStock);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return HandleFailure(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, null);
    }

    [HttpPut("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest request)
    {
        var command = new UpdateProductPriceCommand(id, request.NewPrice);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return HandleFailure(result.Error);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return HandleFailure(result.Error);

        return NoContent();
    }

    // Helper para mapear Error para HTTP Status
    private IActionResult HandleFailure(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
            ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
            ErrorType.Conflict => Conflict(new { error.Code, error.Message }),
            ErrorType.Unauthorized => Unauthorized(new { error.Code, error.Message }),
            ErrorType.Forbidden => Forbid(),
            _ => StatusCode(500, new { error.Code, error.Message })
        };
    }
}
```

---

## 7. Testes Unitários

### Teste de Handler

```csharp
public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateProductCommandHandler(
            _repositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mock.Of<ILogger<CreateProductCommandHandler>>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            "PROD-001",
            "Product 1",
            null,
            99.90m,
            categoryId,
            10);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Category.Create("Category 1"));

        _repositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicatedSku_ShouldReturnConflictError()
    {
        // Arrange
        var command = new CreateProductCommand("PROD-001", "Product 1", null, 99.90m, Guid.NewGuid(), 10);

        _repositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Product.Create(command.Sku, "Existing", 50m, 5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("SKU_DUPLICATED");
    }
}
```

**Estes exemplos cobrem todo o ciclo de CQRS + MediatR no seu sistema!** 🚀
