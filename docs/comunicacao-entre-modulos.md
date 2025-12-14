# Guia de Comunicação Entre Módulos

## E-commerce Modular Monolith - Desacoplamento e Integração

---

## 1. Princípios Fundamentais

### 1.1 Regras de Ouro

✅ **PERMITIDO:**

- Módulos podem depender de **Contracts** de outros módulos
- Módulos se comunicam via **Eventos de Integração**
- Módulos podem consultar dados via **APIs públicas** (queries)
- Uso de **Mediator** para comunicação indireta

❌ **PROIBIDO:**

- Dependência direta entre camadas Core/Application de módulos diferentes
- Referência direta a Entities de outro módulo
- Chamadas diretas a Repositories de outro módulo
- Transações distribuídas entre módulos (cada módulo gerencia sua transação)

### 1.2 Padrões de Comunicação

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│   Orders    │────────▶│   Events    │◀────────│  Payments   │
│   Module    │         │    Bus      │         │   Module    │
└─────────────┘         └─────────────┘         └─────────────┘
       │                       │                        │
       │                       │                        │
       ▼                       ▼                        ▼
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│ Orders.DB   │         │ Outbox/     │         │ Payments.DB │
│             │         │ Inbox       │         │             │
└─────────────┘         └─────────────┘         └─────────────┘
```

---

## 2. Cenários Práticos e Soluções

### Cenário 1: Orders precisa validar se o Product existe no Catalog

#### ❌ ERRADO - Acoplamento Direto

```csharp
// ❌ Orders.Application fazendo referência direta ao Catalog.Core
using Ecommerce.Modules.Catalog.Core.Repositories;

public class CreateOrderCommandHandler
{
    private readonly IProductRepository _productRepository; // ❌ ERRADO!

    public async Task<Result> Handle(CreateOrderCommand command)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId);
        // ...
    }
}
```

#### ✅ CORRETO - Usando Contracts e Mediator

**1. Definir contrato público no Catalog.Contracts:**

```csharp
// Ecommerce.Modules.Catalog.Contracts/Queries/GetProductByIdQuery.cs
namespace Ecommerce.Modules.Catalog.Contracts.Queries
{
    public record GetProductByIdQuery(Guid ProductId) : IQuery<ProductPublicDto>;

    public record ProductPublicDto(
        Guid Id,
        string Name,
        string Sku,
        decimal Price,
        int Stock,
        bool IsActive
    );
}
```

**2. Implementar handler no Catalog.Application:**

```csharp
// Ecommerce.Modules.Catalog.Application/Queries/GetProductById/GetProductByIdQueryHandler.cs
namespace Ecommerce.Modules.Catalog.Application.Queries.GetProductById
{
    internal class GetProductByIdQueryHandler
        : IQueryHandler<GetProductByIdQuery, ProductPublicDto>
    {
        private readonly IProductRepository _productRepository;

        public async Task<ProductPublicDto> Handle(GetProductByIdQuery query)
        {
            var product = await _productRepository.GetByIdAsync(query.ProductId);

            if (product == null)
                return null;

            return new ProductPublicDto(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                product.Stock,
                product.IsActive
            );
        }
    }
}
```

**3. Usar no Orders.Application via Mediator:**

```csharp
// Ecommerce.Modules.Orders.Application/Commands/CreateOrder/CreateOrderCommandHandler.cs
using Ecommerce.Modules.Catalog.Contracts.Queries; // ✅ Apenas Contracts!
using MediatR;

namespace Ecommerce.Modules.Orders.Application.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result>
    {
        private readonly IMediator _mediator;
        private readonly IOrderRepository _orderRepository;

        public async Task<Result> Handle(CreateOrderCommand command)
        {
            // ✅ Comunicação via Mediator
            var product = await _mediator.Send(
                new GetProductByIdQuery(command.ProductId)
            );

            if (product == null)
                return Result.Fail("Product not found");

            if (!product.IsActive)
                return Result.Fail("Product is not active");

            if (product.Stock < command.Quantity)
                return Result.Fail("Insufficient stock");

            // Criar pedido com snapshot do produto
            var order = Order.Create(
                userId: command.UserId,
                items: command.Items.Select(i => new OrderItem(
                    productId: i.ProductId,
                    productSnapshot: new {
                        Name = product.Name,
                        Sku = product.Sku,
                        Price = product.Price
                    },
                    quantity: i.Quantity,
                    unitPrice: product.Price
                ))
            );

            await _orderRepository.AddAsync(order);

            return Result.Ok();
        }
    }
}
```

**Benefícios:**

- ✅ Orders não conhece detalhes internos do Catalog
- ✅ Fácil de testar (mock do Mediator)
- ✅ Catalog pode mudar internamente sem afetar Orders
- ✅ Contrato explícito e versionável

---

### Cenário 2: Payments precisa notificar Orders quando pagamento é aprovado

#### ❌ ERRADO - Chamada Direta

```csharp
// ❌ Payments.Application chamando diretamente Orders.Application
using Ecommerce.Modules.Orders.Application.Services;

public class PaymentCapturedEventHandler
{
    private readonly IOrderService _orderService; // ❌ ERRADO!

    public async Task Handle(PaymentCapturedEvent @event)
    {
        await _orderService.MarkAsPaid(@event.OrderId); // ❌ ACOPLAMENTO!
    }
}
```

#### ✅ CORRETO - Usando Eventos de Integração

**1. Definir evento no Payments.Contracts:**

```csharp
// Ecommerce.Modules.Payments.Contracts/Events/PaymentCapturedIntegrationEvent.cs
namespace Ecommerce.Modules.Payments.Contracts.Events
{
    public record PaymentCapturedIntegrationEvent(
        Guid PaymentId,
        Guid OrderId,
        decimal Amount,
        string PaymentMethod,
        DateTime CapturedAt
    ) : IIntegrationEvent;
}
```

**2. Publicar evento no Payments.Application:**

```csharp
// Ecommerce.Modules.Payments.Application/EventHandlers/PaymentCapturedEventHandler.cs
namespace Ecommerce.Modules.Payments.Application.EventHandlers
{
    internal class PaymentCapturedEventHandler
        : IDomainEventHandler<PaymentCapturedEvent>
    {
        private readonly IEventBus _eventBus;

        public async Task Handle(PaymentCapturedEvent domainEvent)
        {
            // Publicar evento de integração
            var integrationEvent = new PaymentCapturedIntegrationEvent(
                domainEvent.PaymentId,
                domainEvent.OrderId,
                domainEvent.Amount,
                domainEvent.PaymentMethod,
                DateTime.UtcNow
            );

            await _eventBus.PublishAsync(integrationEvent);
        }
    }
}
```

**3. Consumir evento no Orders.Application:**

```csharp
// Ecommerce.Modules.Orders.Application/IntegrationEventHandlers/PaymentCapturedIntegrationEventHandler.cs
using Ecommerce.Modules.Payments.Contracts.Events; // ✅ Apenas Contracts!

namespace Ecommerce.Modules.Orders.Application.IntegrationEventHandlers
{
    internal class PaymentCapturedIntegrationEventHandler
        : IIntegrationEventHandler<PaymentCapturedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        public async Task Handle(PaymentCapturedIntegrationEvent @event)
        {
            var order = await _orderRepository.GetByIdAsync(@event.OrderId);

            if (order == null)
            {
                // Log warning - idempotência
                return;
            }

            // Atualizar status do pedido
            order.MarkAsPaid(@event.CapturedAt);

            await _orderRepository.UpdateAsync(order);

            // Publicar evento de domínio do Orders
            var orderPaidEvent = new OrderPaidEvent(order.Id);
            await _mediator.Publish(orderPaidEvent);
        }
    }
}
```

**Implementação do Event Bus (Outbox Pattern):**

```csharp
// Ecommerce.Shared/Events/EventBus.cs
public class EventBus : IEventBus
{
    private readonly DbContext _dbContext;

    public async Task PublishAsync<T>(T integrationEvent) where T : IIntegrationEvent
    {
        // Salvar no Outbox
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = typeof(T).Name,
            Payload = JsonSerializer.Serialize(integrationEvent),
            OccurredAt = DateTime.UtcNow,
            ProcessedAt = null
        };

        _dbContext.Set<OutboxMessage>().Add(outboxMessage);
        await _dbContext.SaveChangesAsync();
    }
}

// Background Job para processar Outbox
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingMessages = await _dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccurredAt)
                .Take(100)
                .ToListAsync();

            foreach (var message in pendingMessages)
            {
                try
                {
                    // Deserializar e processar
                    var eventType = Type.GetType(message.EventType);
                    var @event = JsonSerializer.Deserialize(message.Payload, eventType);

                    // Chamar handlers registrados
                    await _mediator.Publish(@event);

                    // Marcar como processado
                    message.ProcessedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log error, implementar retry
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

**Benefícios:**

- ✅ Desacoplamento total entre módulos
- ✅ Comunicação assíncrona e confiável
- ✅ Resiliência a falhas (retry automático)
- ✅ Auditoria de eventos
- ✅ Facilita migração futura para message broker (RabbitMQ, etc)

---

### Cenário 3: Cart precisa verificar estoque disponível no Catalog

#### ✅ CORRETO - Query via Mediator + Evento para Reserva

**1. Validar estoque via Query:**

```csharp
// Ecommerce.Modules.Cart.Application/Commands/AddItemToCart/AddItemToCartCommandHandler.cs
using Ecommerce.Modules.Catalog.Contracts.Queries;

public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand>
{
    private readonly IMediator _mediator;
    private readonly ICartRepository _cartRepository;

    public async Task<Result> Handle(AddItemToCartCommand command)
    {
        // ✅ Consultar produto via Mediator
        var product = await _mediator.Send(
            new GetProductByIdQuery(command.ProductId)
        );

        if (product == null)
            return Result.Fail("Product not found");

        if (product.Stock < command.Quantity)
            return Result.Fail("Insufficient stock");

        var cart = await _cartRepository.GetActiveCartAsync(command.UserId);

        cart.AddItem(
            productId: product.Id,
            quantity: command.Quantity,
            unitPrice: product.Price,
            productSnapshot: product // Snapshot para não depender do Catalog depois
        );

        await _cartRepository.UpdateAsync(cart);

        return Result.Ok();
    }
}
```

**2. Reservar estoque no Checkout via Comando:**

```csharp
// Ecommerce.Modules.Catalog.Contracts/Commands/ReserveStockCommand.cs
public record ReserveStockCommand(
    Guid ProductId,
    int Quantity,
    string ReferenceType,
    Guid ReferenceId,
    DateTime ExpiresAt
) : ICommand<Result>;
```

```csharp
// Ecommerce.Modules.Cart.Application/Commands/Checkout/CheckoutCommandHandler.cs
public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand>
{
    private readonly IMediator _mediator;

    public async Task<Result> Handle(CheckoutCommand command)
    {
        var cart = await _cartRepository.GetByIdAsync(command.CartId);

        // Reservar estoque para cada item
        foreach (var item in cart.Items)
        {
            var reserveResult = await _mediator.Send(new ReserveStockCommand(
                ProductId: item.ProductId,
                Quantity: item.Quantity,
                ReferenceType: "Cart",
                ReferenceId: cart.Id,
                ExpiresAt: DateTime.UtcNow.AddMinutes(15)
            ));

            if (reserveResult.IsFailure)
                return Result.Fail($"Failed to reserve stock for {item.ProductName}");
        }

        // Continuar com checkout...
        return Result.Ok();
    }
}
```

**3. Implementar no Catalog.Application:**

```csharp
// Ecommerce.Modules.Catalog.Application/Commands/ReserveStock/ReserveStockCommandHandler.cs
internal class ReserveStockCommandHandler : ICommandHandler<ReserveStockCommand>
{
    private readonly IStockRepository _stockRepository;

    public async Task<Result> Handle(ReserveStockCommand command)
    {
        var product = await _stockRepository.GetByIdAsync(command.ProductId);

        if (product == null)
            return Result.Fail("Product not found");

        // Validar estoque disponível
        var availableStock = product.Stock - product.ReservedStock;
        if (availableStock < command.Quantity)
            return Result.Fail("Insufficient stock");

        // Criar reserva
        var reservation = StockReservation.Create(
            productId: command.ProductId,
            quantity: command.Quantity,
            referenceType: command.ReferenceType,
            referenceId: command.ReferenceId,
            expiresAt: command.ExpiresAt
        );

        await _stockRepository.AddReservationAsync(reservation);

        // Atualizar estoque reservado
        product.ReserveStock(command.Quantity);
        await _stockRepository.UpdateAsync(product);

        return Result.Ok();
    }
}
```

---

### Cenário 4: Orders precisa de dados do User (endereço de entrega)

#### ✅ CORRETO - Snapshot de Dados

**Princípio:** Orders não deve depender de Users em runtime. Deve salvar snapshot dos dados necessários.

```csharp
// Ecommerce.Modules.Orders.Application/Commands/CreateOrder/CreateOrderCommandHandler.cs
using Ecommerce.Modules.Users.Contracts.Queries;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand>
{
    private readonly IMediator _mediator;
    private readonly IOrderRepository _orderRepository;

    public async Task<Result> Handle(CreateOrderCommand command)
    {
        // ✅ Buscar endereço via Mediator
        var address = await _mediator.Send(
            new GetUserAddressQuery(command.UserId, command.AddressId)
        );

        if (address == null)
            return Result.Fail("Address not found");

        // ✅ Criar snapshot do endereço (JSON)
        var shippingAddressSnapshot = new
        {
            RecipientName = address.RecipientName,
            Street = address.Street,
            Number = address.Number,
            Complement = address.Complement,
            Neighborhood = address.Neighborhood,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };

        // Criar pedido com snapshot
        var order = Order.Create(
            userId: command.UserId,
            shippingAddress: shippingAddressSnapshot, // Salvo como JSONB
            items: command.Items
        );

        await _orderRepository.AddAsync(order);

        return Result.Ok();
    }
}
```

**Por que Snapshot?**

- ✅ Endereço pode ser alterado/deletado pelo usuário
- ✅ Pedido deve manter dados históricos
- ✅ Auditoria e compliance
- ✅ Orders não depende de Users estar disponível

---

### Cenário 5: Coupons precisa validar produtos do Cart

#### ✅ CORRETO - Cart passa dados necessários via Command

```csharp
// Ecommerce.Modules.Coupons.Contracts/Commands/ValidateCouponCommand.cs
public record ValidateCouponCommand(
    string CouponCode,
    Guid? UserId,
    decimal CartSubtotal,
    List<CartItemDto> Items
) : ICommand<CouponValidationResult>;

public record CartItemDto(
    Guid ProductId,
    Guid CategoryId,
    int Quantity,
    decimal UnitPrice
);

public record CouponValidationResult(
    bool IsValid,
    string ErrorCode,
    string ErrorMessage,
    decimal DiscountAmount,
    Guid? CouponId
);
```

```csharp
// Ecommerce.Modules.Cart.Application/Commands/ApplyCoupon/ApplyCouponCommandHandler.cs
using Ecommerce.Modules.Coupons.Contracts.Commands;

public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand>
{
    private readonly IMediator _mediator;
    private readonly ICartRepository _cartRepository;

    public async Task<Result> Handle(ApplyCouponCommand command)
    {
        var cart = await _cartRepository.GetByIdAsync(command.CartId);

        // Preparar dados dos itens
        var cartItems = cart.Items.Select(i => new CartItemDto(
            ProductId: i.ProductId,
            CategoryId: i.ProductSnapshot.CategoryId,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice
        )).ToList();

        // ✅ Validar cupom via Mediator
        var validation = await _mediator.Send(new ValidateCouponCommand(
            CouponCode: command.CouponCode,
            UserId: cart.UserId,
            CartSubtotal: cart.CalculateSubtotal(),
            Items: cartItems
        ));

        if (!validation.IsValid)
            return Result.Fail(validation.ErrorMessage);

        // Aplicar cupom no carrinho
        cart.ApplyCoupon(
            couponId: validation.CouponId.Value,
            couponCode: command.CouponCode,
            discountAmount: validation.DiscountAmount
        );

        await _cartRepository.UpdateAsync(cart);

        return Result.Ok();
    }
}
```

**Implementação no Coupons.Application:**

```csharp
// Ecommerce.Modules.Coupons.Application/Commands/ValidateCoupon/ValidateCouponCommandHandler.cs
internal class ValidateCouponCommandHandler
    : ICommandHandler<ValidateCouponCommand, CouponValidationResult>
{
    private readonly ICouponRepository _couponRepository;

    public async Task<CouponValidationResult> Handle(ValidateCouponCommand command)
    {
        var coupon = await _couponRepository.GetByCodeAsync(command.CouponCode);

        if (coupon == null)
            return new CouponValidationResult(false, "NOT_FOUND", "Coupon not found", 0, null);

        // Validações...
        if (coupon.Status != CouponStatus.Active)
            return new CouponValidationResult(false, "INACTIVE", "Coupon is not active", 0, null);

        if (command.CartSubtotal < coupon.MinPurchaseAmount)
            return new CouponValidationResult(
                false,
                "MIN_AMOUNT",
                $"Minimum purchase of {coupon.MinPurchaseAmount} required",
                0,
                null
            );

        // Validar elegibilidade de produtos/categorias
        if (coupon.Scope == CouponScope.Products)
        {
            var eligibleProducts = await _couponRepository.GetEligibleProductsAsync(coupon.Id);
            var hasEligibleProducts = command.Items.Any(i => eligibleProducts.Contains(i.ProductId));

            if (!hasEligibleProducts)
                return new CouponValidationResult(
                    false,
                    "NO_ELIGIBLE_PRODUCTS",
                    "No eligible products in cart",
                    0,
                    null
                );
        }

        // Calcular desconto
        var discount = CalculateDiscount(coupon, command.CartSubtotal);

        return new CouponValidationResult(true, null, null, discount, coupon.Id);
    }
}
```

---

## 3. Padrões de Registro e Configuração

### 3.1 Registro de Módulos no Startup

```csharp
// Ecommerce.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registrar módulos
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddCartModule(builder.Configuration);
builder.Services.AddOrdersModule(builder.Configuration);
builder.Services.AddPaymentsModule(builder.Configuration);
builder.Services.AddCouponsModule(builder.Configuration);

// Registrar infraestrutura compartilhada
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddEventBus();
builder.Services.AddMediatR(cfg => {
    // Registrar assemblies de todos os módulos
    cfg.RegisterServicesFromAssembly(typeof(UsersModule).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CatalogModule).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CartModule).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(OrdersModule).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(PaymentsModule).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CouponsModule).Assembly);
});
```

### 3.2 Extension Methods por Módulo

```csharp
// Ecommerce.Modules.Catalog.Infrastructure/Extensions/ServiceCollectionExtensions.cs
namespace Ecommerce.Modules.Catalog.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")
                )
            );

            // Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IStockRepository, StockRepository>();

            // Services
            services.AddScoped<IStockManagementService, StockManagementService>();
            services.AddScoped<IProductSearchService, ProductSearchService>();

            // MediatR Handlers são registrados automaticamente

            return services;
        }
    }
}
```

---

## 4. Estrutura de Pastas Recomendada

```
Modules/
├── Users/
│   ├── Ecommerce.Modules.Users.Core/           # ❌ Não referenciado por outros módulos
│   ├── Ecommerce.Modules.Users.Application/    # ❌ Não referenciado por outros módulos
│   ├── Ecommerce.Modules.Users.Infrastructure/ # ❌ Não referenciado por outros módulos
│   └── Ecommerce.Modules.Users.Contracts/      # ✅ Referenciado por outros módulos
│       ├── Queries/
│       │   ├── GetUserByIdQuery.cs
│       │   └── GetUserAddressQuery.cs
│       ├── Events/
│       │   └── UserRegisteredIntegrationEvent.cs
│       └── DTOs/
│           └── UserPublicDto.cs
│
├── Catalog/
│   ├── Ecommerce.Modules.Catalog.Core/
│   ├── Ecommerce.Modules.Catalog.Application/
│   ├── Ecommerce.Modules.Catalog.Infrastructure/
│   └── Ecommerce.Modules.Catalog.Contracts/    # ✅ Referenciado por Cart, Orders
│       ├── Queries/
│       │   ├── GetProductByIdQuery.cs
│       │   └── SearchProductsQuery.cs
│       ├── Commands/
│       │   ├── ReserveStockCommand.cs
│       │   └── ReleaseStockCommand.cs
│       └── Events/
│           ├── ProductCreatedIntegrationEvent.cs
│           └── StockChangedIntegrationEvent.cs
```

---

## 5. Checklist de Validação

Ao adicar comunicação entre módulos, valide:

- [ ] **Contracts**: O módulo expõe apenas DTOs e interfaces necessárias?
- [ ] **Sem referências diretas**: Nenhum módulo referencia Core/Application de outro?
- [ ] **Mediator**: Queries e Commands são enviados via MediatR?
- [ ] **Eventos**: Ações importantes publicam eventos de integração?
- [ ] **Snapshots**: Dados históricos são salvos como snapshots?
- [ ] **Idempotência**: Handlers de eventos são idempotentes?
- [ ] **Resiliência**: Existe retry e tratamento de erros?
- [ ] **Testes**: É possível testar o módulo isoladamente com mocks?

---

## 6. Comparação: Antes x Depois

### ❌ ANTES (Acoplado)

```
Orders.Application ──────▶ Catalog.Core.Repositories
       │                          │
       └──────────────────────────┘
     (Dependência direta e frágil)
```

### ✅ DEPOIS (Desacoplado)

```
Orders.Application ──────▶ Catalog.Contracts
       │                          │
       │                          │
       ▼                          ▼
    Mediator ◀──────────────▶ Catalog.Application
       │
       └──────▶ Event Bus ◀────── Catalog.Application
                   │
                   └──────▶ Orders.Application
              (Comunicação via eventos)
```

---

## 7. Resumo dos Padrões

| Necessidade           | Padrão                    | Exemplo                        |
| --------------------- | ------------------------- | ------------------------------ |
| **Consultar dados**   | Query via Mediator        | Orders consultando Product     |
| **Executar ação**     | Command via Mediator      | Cart reservando Stock          |
| **Notificar mudança** | Integration Event         | Payment notificando Order      |
| **Dados históricos**  | Snapshot (JSONB)          | Order salvando Address         |
| **Validação**         | Command retornando Result | Coupon validando elegibilidade |

---

## 8. Vantagens desta Abordagem

✅ **Testabilidade**: Cada módulo pode ser testado isoladamente  
✅ **Manutenibilidade**: Mudanças internas não afetam outros módulos  
✅ **Escalabilidade**: Fácil evoluir para microserviços no futuro  
✅ **Clareza**: Contratos explícitos de comunicação  
✅ **Resiliência**: Falhas em um módulo não derrubam outros  
✅ **Auditoria**: Todos os eventos são registrados  
✅ **Versionamento**: Contratos podem ser versionados independentemente

---

**Última atualização**: 2025-12-12  
**Versão**: 1.0
