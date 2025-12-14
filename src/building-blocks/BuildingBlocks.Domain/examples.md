# Exemplos Práticos - BuildingBlocks.Domain

Exemplos baseados no sistema de e-commerce modular monolith.

---

## 📋 Índice

1. [Entities e Aggregate Roots](#1-entities-e-aggregate-roots)
2. [Value Objects](#2-value-objects)
3. [Domain Events](#3-domain-events)
4. [Integration Events](#4-integration-events)
5. [Smart Enums](#5-smart-enums)
6. [Result Pattern](#6-result-pattern)
7. [Repositories](#7-repositories)
8. [Domain Exceptions](#8-domain-exceptions)

---

## 1. Entities e Aggregate Roots

### Exemplo 1: Produto (Catalog Module)

```csharp
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Models;

namespace Ecommerce.Modules.Catalog.Core.Products;

public class Product : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    private readonly List<ProductImage> _images = new();

    public string Sku { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public int ReservedStock { get; private set; }
    public ProductStatus Status { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Product() { } // EF Core

    public static Product Create(
        string sku,
        string name,
        decimal price,
        int initialStock)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required", "INVALID_SKU");

        if (price < 0)
            throw new DomainException("Price cannot be negative", "INVALID_PRICE");

        var product = new Product
        {
            Sku = sku,
            Name = name,
            Slug = GenerateSlug(name),
            Price = price,
            Stock = initialStock,
            ReservedStock = 0,
            Status = ProductStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, sku));
        return product;
    }

    public Result ReserveStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Fail("Quantity must be positive", "INVALID_QUANTITY");

        var availableStock = Stock - ReservedStock;
        if (availableStock < quantity)
            return Result.Fail(
                $"Insufficient stock. Available: {availableStock}, Requested: {quantity}",
                "INSUFFICIENT_STOCK");

        ReservedStock += quantity;
        AddDomainEvent(new StockReservedEvent(Id, quantity));

        return Result.Ok();
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity > ReservedStock)
            throw new DomainException("Cannot release more than reserved", "INVALID_RELEASE");

        ReservedStock -= quantity;
        AddDomainEvent(new StockReleasedEvent(Id, quantity));
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new DomainException("Price cannot be negative", "INVALID_PRICE");

        var oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void Publish()
    {
        if (Stock <= 0)
            throw new DomainException("Cannot publish product with no stock", "NO_STOCK");

        Status = ProductStatus.Active;
        AddDomainEvent(new ProductPublishedEvent(Id));
    }

    public void Delete() => DeletedAt = DateTime.UtcNow;
    public void Restore() => DeletedAt = null;

    private static string GenerateSlug(string name)
    {
        // Simplificado - use biblioteca real em produção
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ã", "a")
            .Replace("õ", "o");
    }
}
```

### Exemplo 2: Pedido (Orders Module)

```csharp
namespace Ecommerce.Modules.Orders.Core.Orders;

public class Order : AggregateRoot, IAuditableEntity
{
    private readonly List<OrderItem> _items = new();

    public string OrderNumber { get; private set; }
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public decimal Total { get; private set; }

    // Snapshot do endereço (JSONB no PostgreSQL)
    public AddressSnapshot ShippingAddress { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }

    private Order() { }

    public static Order Create(
        Guid userId,
        List<OrderItem> items,
        AddressSnapshot shippingAddress,
        decimal shippingAmount)
    {
        if (!items.Any())
            throw new DomainException("Order must have at least one item", "NO_ITEMS");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress,
            ShippingAmount = shippingAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            order._items.Add(item);
        }

        order.CalculateTotals();
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, userId));

        return order;
    }

    public Result MarkAsPaid(DateTime paidAt)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.PaymentProcessing)
            return Result.Fail($"Cannot mark as paid. Current status: {Status.Name}", "INVALID_STATUS");

        Status = OrderStatus.Paid;
        PaidAt = paidAt;

        AddDomainEvent(new OrderPaidEvent(Id, Total, paidAt));
        return Result.Ok();
    }

    public Result Cancel(string reason)
    {
        if (!Status.CanBeCancelled())
            return Result.Fail($"Cannot cancel order in status: {Status.Name}", "CANNOT_CANCEL");

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledEvent(Id, reason));

        return Result.Ok();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0 || discountAmount > Subtotal)
            throw new DomainException("Invalid discount amount", "INVALID_DISCOUNT");

        DiscountAmount = discountAmount;
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        Subtotal = _items.Sum(i => i.UnitPrice * i.Quantity);
        Total = Subtotal - DiscountAmount + ShippingAmount;
    }

    private static string GenerateOrderNumber()
    {
        var year = DateTime.UtcNow.ToString("yy");
        var random = Random.Shared.Next(100000, 999999);
        return $"{year}-{random}";
    }
}
```

---

## 2. Value Objects

### Exemplo 1: Endereço

```csharp
namespace Ecommerce.Modules.Users.Core.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; }
    public string Number { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address() { } // EF Core

    public Address(
        string street,
        string number,
        string city,
        string state,
        string postalCode,
        string country = "BR")
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street is required");

        if (!IsValidPostalCode(postalCode))
            throw new DomainException("Invalid postal code format");

        Street = street;
        Number = number;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    private static bool IsValidPostalCode(string postalCode)
    {
        // Formato: 12345-678 ou 12345678
        var cleaned = postalCode.Replace("-", "");
        return cleaned.Length == 8 && cleaned.All(char.IsDigit);
    }

    public string GetFormattedPostalCode()
    {
        var cleaned = PostalCode.Replace("-", "");
        return $"{cleaned.Substring(0, 5)}-{cleaned.Substring(5)}";
    }
}
```

### Exemplo 2: Money (Valor Monetário)

```csharp
namespace BuildingBlocks.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { }

    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");

        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public static Money Zero(string currency = "BRL") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot subtract money with different currencies");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
}
```

---

## 3. Domain Events

```csharp
namespace Ecommerce.Modules.Orders.Core.Events;

// Evento de domínio
public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(Guid orderId, Guid userId, decimal totalAmount)
    {
        OrderId = orderId;
        UserId = userId;
        TotalAmount = totalAmount;
    }
}

// Handler (mesmo módulo)
internal class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    private readonly INotificationService _notificationService;

    public OrderCreatedEventHandler(
        ILogger<OrderCreatedEventHandler> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} created for user {UserId}",
            notification.OrderId,
            notification.UserId);

        // Enviar notificação para o usuário
        await _notificationService.NotifyOrderCreatedAsync(
            notification.UserId,
            notification.OrderId,
            cancellationToken);
    }
}
```

---

## 4. Integration Events

```csharp
// Payments.Contracts (pode ser consumido por outros módulos)
namespace Ecommerce.Modules.Payments.Contracts.Events;

public record PaymentCapturedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    DateTime CapturedAt
) : IntegrationEvent("payments");

// Converter Domain Event → Integration Event
namespace Ecommerce.Modules.Payments.Application.EventHandlers;

internal class PaymentCapturedDomainEventHandler
    : INotificationHandler<PaymentCapturedEvent>
{
    private readonly IEventBus _eventBus;

    public async Task Handle(PaymentCapturedEvent domainEvent, CancellationToken ct)
    {
        // Converter para Integration Event
        var integrationEvent = new PaymentCapturedIntegrationEvent(
            domainEvent.PaymentId,
            domainEvent.OrderId,
            domainEvent.Amount,
            domainEvent.PaymentMethod,
            DateTime.UtcNow
        );

        // Publicar (salva no Outbox: shared.domain_events)
        await _eventBus.PublishAsync(integrationEvent, ct);
    }
}

// Consumir em outro módulo (Orders)
namespace Ecommerce.Modules.Orders.Application.IntegrationEventHandlers;

internal class PaymentCapturedIntegrationEventHandler
    : IIntegrationEventHandler<PaymentCapturedIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;

    public async Task Handle(PaymentCapturedIntegrationEvent @event, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(@event.OrderId, ct);

        if (order == null)
            return; // Idempotência

        order.MarkAsPaid(@event.CapturedAt);
        await _orderRepository.UnitOfWork.SaveChangesAsync(ct);
    }
}
```

---

## 5. Smart Enums

```csharp
namespace Ecommerce.Modules.Orders.Core.Enums;

public class OrderStatus : Enumeration
{
    public static OrderStatus Pending = new(1, "PENDING");
    public static OrderStatus PaymentProcessing = new(2, "PAYMENT_PROCESSING");
    public static OrderStatus Paid = new(3, "PAID");
    public static OrderStatus Preparing = new(4, "PREPARING");
    public static OrderStatus Shipped = new(5, "SHIPPED");
    public static OrderStatus Delivered = new(6, "DELIVERED");
    public static OrderStatus Cancelled = new(7, "CANCELLED");
    public static OrderStatus Refunded = new(8, "REFUNDED");

    private OrderStatus(int id, string name) : base(id, name) { }

    // Métodos de negócio
    public bool CanBeCancelled() =>
        this == Pending || this == PaymentProcessing || this == Paid;

    public bool CanBeShipped() =>
        this == Paid || this == Preparing;

    public bool IsCompleted() =>
        this == Delivered || this == Cancelled || this == Refunded;

    public bool RequiresPayment() =>
        this == Pending || this == PaymentProcessing;
}

// Uso:
var status = OrderStatus.Pending;

if (status.CanBeCancelled())
{
    order.Cancel("Customer requested");
}

// Conversão do PostgreSQL ENUM
var statusFromDb = OrderStatus.FromName("PAID"); // Retorna OrderStatus.Paid
```

---

## 6. Result Pattern

```csharp
// Command Handler
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public async Task<Result<Guid>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        // Validar itens
        if (!command.Items.Any())
            return Result.Fail<Guid>("Order must have at least one item", "NO_ITEMS");

        // Validar estoque
        foreach (var item in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);

            if (product == null)
                return Result.Fail<Guid>($"Product {item.ProductId} not found", "PRODUCT_NOT_FOUND");

            var reserveResult = product.ReserveStock(item.Quantity);
            if (reserveResult.IsFailure)
                return Result.Fail<Guid>(reserveResult.Error!, reserveResult.ErrorCode);
        }

        // Criar pedido
        var order = Order.Create(
            command.UserId,
            command.Items,
            command.ShippingAddress,
            command.ShippingAmount);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(order.Id);
    }
}

// Uso no Controller
[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
{
    var command = request.ToCommand();
    var result = await _mediator.Send(command);

    if (result.IsFailure)
        return BadRequest(new { error = result.Error, code = result.ErrorCode });

    return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, null);
}
```

---

## 7. Repositories

```csharp
// Interface (Core)
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Remove(Product product);
}

// Implementação (Infrastructure)
internal class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku && p.DeletedAt == null, ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken ct = default)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Remove(Product product)
    {
        product.Delete(); // Soft delete
        _context.Products.Update(product);
    }
}
```

---

## 8. Domain Exceptions

```csharp
// Exceções específicas de domínio
namespace Ecommerce.Modules.Catalog.Core.Exceptions;

public class InsufficientStockException : DomainException
{
    public int Available { get; }
    public int Requested { get; }

    public InsufficientStockException(int available, int requested)
        : base($"Insufficient stock. Available: {available}, Requested: {requested}", "INSUFFICIENT_STOCK")
    {
        Available = available;
        Requested = requested;
    }
}

public class InvalidPriceException : DomainException
{
    public InvalidPriceException(decimal price)
        : base($"Invalid price: {price}. Price must be positive.", "INVALID_PRICE")
    {
    }
}

// Uso na entidade
public void UpdatePrice(decimal newPrice)
{
    if (newPrice < 0)
        throw new InvalidPriceException(newPrice);

    Price = newPrice;
}

// Tratamento global (API)
public class DomainExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = ex.Message,
                code = ex.ErrorCode
            });
        }
    }
}
```

---

**Esses exemplos cobrem os principais cenários do seu sistema de e-commerce!** 🚀
