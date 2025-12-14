# BuildingBlocks.Domain

Biblioteca de blocos de construção para arquitetura de **Domain-Driven Design (DDD)** aplicada ao sistema de e-commerce modular monolith.

---

## 📦 Estrutura

```
BuildingBlocks.Domain/
├── Entities/
│   ├── Entity.cs                    # Base com Id e DomainEvents
│   ├── AggregateRoot.cs             # Base com Version (Optimistic Concurrency)
│   ├── IAuditableEntity.cs          # Interface para created_at, updated_at
│   └── ISoftDeletable.cs            # Interface para deleted_at (soft delete)
│
├── Events/
│   ├── IDomainEvent.cs              # Interface para eventos de domínio
│   ├── DomainEvent.cs               # Classe base para eventos de domínio
│   ├── IIntegrationEvent.cs         # Interface para eventos entre módulos
│   └── IntegrationEvent.cs          # Classe base para eventos de integração
│
├── Models/
│   ├── ValueObject.cs               # Base para igualdade estrutural
│   ├── Enumeration.cs               # Smart Enums com comportamento
│   └── Result.cs                    # Result pattern (sem exceções)
│
├── Repositories/
│   ├── IRepository.cs               # Interface marcadora
│   └── IUnitOfWork.cs               # Interface para transações
│
└── Exceptions/
    └── DomainException.cs           # Exceções de regras de negócio
```

---

## 🎯 Conceitos e Uso

### 1. Entities

#### **Entity.cs**

Classe base para todas as entidades do domínio. Fornece:

- ✅ Identificador único via `Guid`
- ✅ Coleção de eventos de domínio
- ✅ Igualdade baseada em identidade

**Exemplo:**

```csharp
public class Product : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Product() { } // EF Core

    public static Product Create(string name, decimal price, int stock)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            Stock = stock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id));
        return product;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new DomainException("Price must be positive");

        Price = newPrice;
        AddDomainEvent(new ProductPriceChangedEvent(Id, newPrice));
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
    }
}
```

#### **AggregateRoot.cs**

Adiciona versionamento para **Optimistic Concurrency Control**:

```csharp
public class Order : AggregateRoot
{
    public string OrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    // Version é incrementado automaticamente via trigger no PostgreSQL
}
```

#### **IAuditableEntity** e **ISoftDeletable**

Interfaces para marcação de entidades com comportamentos específicos:

```csharp
public class Category : Entity, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void Delete() => DeletedAt = DateTime.UtcNow;
    public void Restore() => DeletedAt = null;
}
```

---

### 2. Events

#### **Domain Events (IDomainEvent)**

Eventos publicados **dentro do mesmo módulo**, processados via MediatR:

```csharp
public class ProductCreatedEvent : DomainEvent
{
    public Guid ProductId { get; }

    public ProductCreatedEvent(Guid productId)
    {
        ProductId = productId;
    }
}

// Handler no mesmo módulo
internal class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Atualizar cache, enviar notificação, etc.
    }
}
```

#### **Integration Events (IIntegrationEvent)**

Eventos para **comunicação entre módulos**, salvos no Outbox:

```csharp
// No módulo Payments.Contracts
public record PaymentCapturedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime CapturedAt
) : IntegrationEvent("payments");

// Publicado via EventBus (salvo em shared.domain_events)
await _eventBus.PublishAsync(integrationEvent, cancellationToken);

// Consumido no módulo Orders.Application
internal class PaymentCapturedIntegrationEventHandler
    : IIntegrationEventHandler<PaymentCapturedIntegrationEvent>
{
    public async Task Handle(PaymentCapturedIntegrationEvent @event, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(@event.OrderId);
        order.MarkAsPaid(@event.CapturedAt);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

---

### 3. Models

#### **ValueObject.cs**

Objetos sem identidade, definidos por seus valores:

```csharp
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }

    public Address(string street, string city, string postalCode)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
    }
}

// Uso:
var address1 = new Address("Rua A", "São Paulo", "01234-567");
var address2 = new Address("Rua A", "São Paulo", "01234-567");
var areEqual = address1 == address2; // true
```

#### **Enumeration.cs**

Smart Enums com comportamento (substitui `enum` C#):

```csharp
public class OrderStatus : Enumeration
{
    public static OrderStatus Pending = new(1, "PENDING");
    public static OrderStatus Paid = new(2, "PAID");
    public static OrderStatus Shipped = new(3, "SHIPPED");
    public static OrderStatus Delivered = new(4, "DELIVERED");
    public static OrderStatus Cancelled = new(5, "CANCELLED");

    private OrderStatus(int id, string name) : base(id, name) { }

    public bool CanBeCancelled() => this == Pending || this == Paid;
    public bool IsCompleted() => this == Delivered || this == Cancelled;
}

// Uso:
var status = OrderStatus.Pending;
if (status.CanBeCancelled())
{
    order.Cancel();
}

var statusFromDb = OrderStatus.FromName("PAID"); // Converte string do PostgreSQL
```

#### **Result.cs**

Pattern para evitar exceções em fluxos de negócio:

```csharp
public Result<Order> CreateOrder(CreateOrderCommand command)
{
    if (command.Items.Count == 0)
        return Result.Fail<Order>("Order must have at least one item");

    var order = Order.Create(command.UserId, command.Items);
    return Result.Ok(order);
}

// Uso:
var result = CreateOrder(command);
if (result.IsFailure)
{
    return BadRequest(result.Error);
}

var order = result.Value;
```

---

### 4. Repositories

#### **IRepository<TEntity>**

Interface marcadora para repositórios específicos de domínio:

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Remove(Product product);
}
```

#### **IUnitOfWork**

Interface para transações (implementado pelo DbContext):

```csharp
public class CatalogDbContext : DbContext, IUnitOfWork
{
    public DbSet<Product> Products { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publicar eventos de domínio
        await DispatchDomainEventsAsync(cancellationToken);

        // Salvar mudanças
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

---

### 5. Exceptions

#### **DomainException**

Exceções para regras de negócio violadas:

```csharp
public class InsufficientStockException : DomainException
{
    public InsufficientStockException(int available, int requested)
        : base($"Insufficient stock. Available: {available}, Requested: {requested}", "INSUFFICIENT_STOCK")
    {
    }
}

// Uso:
public void ReserveStock(int quantity)
{
    var available = Stock - ReservedStock;
    if (available < quantity)
        throw new InsufficientStockException(available, quantity);

    ReservedStock += quantity;
}
```

---

## 🔗 Integração com PostgreSQL

As classes estão alinhadas com o schema PostgreSQL do seu sistema:

| Classe/Interface             | PostgreSQL Mapping                                          |
| ---------------------------- | ----------------------------------------------------------- |
| `Entity.Id`                  | Coluna `id UUID PRIMARY KEY`                                |
| `AggregateRoot.Version`      | Coluna `version INT`, trigger `trigger_increment_version()` |
| `IAuditableEntity.CreatedAt` | Coluna `created_at TIMESTAMPTZ DEFAULT NOW()`               |
| `IAuditableEntity.UpdatedAt` | Coluna `updated_at`, trigger `trigger_set_timestamp()`      |
| `ISoftDeletable.DeletedAt`   | Coluna `deleted_at TIMESTAMPTZ`                             |
| `Enumeration`                | Tipos `shared.order_status`, `shared.payment_status`, etc.  |
| Domain Events                | Armazenados em `shared.domain_events` (Outbox)              |

---

## 📚 Boas Práticas

### ✅ DO:

- Use `Result<T>` para operações que podem falhar por razões de negócio
- Lance `DomainException` apenas para violações críticas de regras
- Mantenha entidades focadas em regras de domínio
- Use Value Objects para conceitos sem identidade
- Publique Domain Events para efeitos colaterais internos
- Use Integration Events para comunicação entre módulos

### ❌ DON'T:

- Não coloque lógica de infraestrutura nas entidades
- Não retorne DTOs diretamente de métodos de domínio
- Não use Domain Events para comunicação entre módulos
- Não crie entidades anêmicas (apenas getters/setters)
- Não exponha coleções mutáveis

---

## 🚀 Exemplo Completo

```csharp
// 1. Entidade de Domínio
public class Cart : AggregateRoot, IAuditableEntity
{
    private readonly List<CartItem> _items = new();

    public Guid UserId { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Cart() { }

    public static Cart Create(Guid userId)
    {
        var cart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        cart.AddDomainEvent(new CartCreatedEvent(cart.Id, userId));
        return cart;
    }

    public Result AddItem(Guid productId, int quantity, decimal price)
    {
        if (quantity <= 0)
            return Result.Fail("Quantity must be positive");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(CartItem.Create(productId, quantity, price));
        }

        AddDomainEvent(new ItemAddedToCartEvent(Id, productId, quantity));
        return Result.Ok();
    }
}

// 2. Value Object
public class CartItem : Entity
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private CartItem() { }

    public static CartItem Create(Guid productId, int quantity, decimal unitPrice)
    {
        return new CartItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    public void IncreaseQuantity(int amount)
    {
        Quantity += amount;
    }
}

// 3. Domain Event
public class ItemAddedToCartEvent : DomainEvent
{
    public Guid CartId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }

    public ItemAddedToCartEvent(Guid cartId, Guid productId, int quantity)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }
}

// 4. Repository
public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
}
```

---

## 📖 Referências

- [Domain-Driven Design (Eric Evans)](https://www.domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design (Vaughn Vernon)](https://vaughnvernon.com/)
- [Microsoft - DDD Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)

---

**Versão**: 1.0.0  
**Data**: 2025-12-13  
**Projeto**: E-commerce Modular Monolith
