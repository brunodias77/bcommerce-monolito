# BuildingBlocks.Infrastructure - Implementação Completa

## 📦 Resumo da Entrega

Implementação completa da **Infrastructure Layer** com **EF Core Interceptors** que fazem a "mágica" de integração com o schema PostgreSQL e **Outbox Pattern** completo para eventos.

---

## 📂 Estrutura Entregue

```
BuildingBlocks.Infrastructure/
│
├── 📁 Persistence/
│   ├── Configurations/
│   │   └── BaseEntityConfiguration.cs     # ✅ Configuração base EF Core
│   │
│   └── Interceptors/                      # 🎯 A "MÁGICA" do Sistema
│       ├── AuditableEntityInterceptor.cs  # ✅ created_at, updated_at
│       ├── SoftDeleteInterceptor.cs       # ✅ Soft delete (deleted_at)
│       ├── OptimisticConcurrencyInterceptor.cs # ✅ version (OCC)
│       └── PublishDomainEventsInterceptor.cs   # ✅ Outbox Pattern
│
├── 📁 Messaging/
│   ├── Outbox/
│   │   ├── OutboxMessage.cs               # ✅ Mapeia shared.domain_events
│   │   └── OutboxProcessor.cs             # ✅ Background job
│   │
│   └── Integration/
│       ├── IEventBus.cs                   # ✅ Interface para eventos
│       └── EventBus.cs                    # ✅ Implementação
│
├── 📁 Services/
│   └── DateTimeProvider.cs                # ✅ Testabilidade de datas
│
├── 📄 BuildingBlocks.Infrastructure.csproj # ✅ Projeto .NET 8
├── 📄 .editorconfig                       # ✅ Configurações
└── 📄 README.md                           # ✅ Documentação completa
```

---

## ✨ Destaques da Implementação

### 1. **Interceptors - Integração Perfeita com PostgreSQL**

Os **4 Interceptors** conectam o EF Core com os triggers do seu schema:

#### **AuditableEntityInterceptor**

```csharp
// Preenche automaticamente
public class Product : AggregateRoot, IAuditableEntity
{
    public DateTime CreatedAt { get; private set; }  // ✅ Preenchido automaticamente
    public DateTime UpdatedAt { get; private set; }  // ✅ Atualizado em cada mudança
}

// PostgreSQL Trigger correspondente:
// CREATE TRIGGER trg_products_updated_at BEFORE UPDATE
// EXECUTE FUNCTION shared.trigger_set_timestamp()
```

#### **SoftDeleteInterceptor**

```csharp
// Converte DELETE em UPDATE
dbContext.Products.Remove(product);
await dbContext.SaveChangesAsync();

// SQL gerado:
// UPDATE catalog.products SET deleted_at = NOW() WHERE id = '...'
// (ao invés de DELETE FROM ...)

// Query Filter automático ignora deletados
var products = await dbContext.Products.ToListAsync();
// SELECT * FROM products WHERE deleted_at IS NULL
```

#### **OptimisticConcurrencyInterceptor**

```csharp
// Incrementa version automaticamente
// Se dois threads modificam simultaneamente → DbUpdateConcurrencyException

// PostgreSQL Trigger correspondente:
// CREATE TRIGGER trg_orders_version BEFORE UPDATE
// EXECUTE FUNCTION shared.trigger_increment_version()
```

#### **PublishDomainEventsInterceptor** ⭐ (O MAIS IMPORTANTE!)

```csharp
// Salva Domain Events no Outbox automaticamente
var product = Product.Create("SKU-001", "Product", 99.90m, 10);
// product.DomainEvents contém ProductCreatedEvent

await _repository.AddAsync(product);
await _unitOfWork.SaveChangesAsync();

// Interceptor automaticamente:
// 1. Coleta todos os Domain Events
// 2. Serializa como JSON
// 3. Salva em shared.domain_events (Outbox)
// 4. Limpa eventos da entidade
// 5. Tudo na mesma transação!
```

---

### 2. **Outbox Pattern Completo**

Sistema robusto de eventos com garantia de entrega:

```
Fluxo:
1. Domain Event levantado → ProductCreatedEvent
2. SaveChangesAsync()
3. PublishDomainEventsInterceptor → salva no Outbox
4. Commit (evento + dados na mesma transação)
5. OutboxProcessor (background job)
   ├─ Busca eventos não processados
   ├─ Deserializa JSON
   ├─ Publica via MediatR
   └─ Marca como processado
6. Handlers de outros módulos recebem
```

#### **OutboxMessage**

```csharp
// Mapeia shared.domain_events do PostgreSQL
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Module { get; set; }         // "catalog", "orders"
    public string EventType { get; set; }      // "ProductCreatedEvent"
    public string Payload { get; set; }        // JSON do evento
    public DateTime? ProcessedAt { get; set; } // null = não processado
    public int RetryCount { get; set; }        // tentativas
}
```

#### **OutboxProcessor**

```csharp
// Background service que processa eventos
builder.Services.AddOutboxProcessor(
    interval: TimeSpan.FromSeconds(5),
    batchSize: 100,
    maxRetries: 3
);

// Funcionalidades:
// ✅ Retry automático em falhas
// ✅ Processamento em batch
// ✅ Dead letter queue (retry_count >= 3)
// ✅ Idempotência via processed_at
```

---

### 3. **Integration Events (EventBus)**

Sistema de comunicação entre módulos:

```csharp
// No módulo Payments
internal class PaymentCapturedEventHandler
    : INotificationHandler<PaymentCapturedEvent>
{
    private readonly IEventBus _eventBus;

    public async Task Handle(PaymentCapturedEvent domainEvent, CancellationToken ct)
    {
        // Converter Domain Event → Integration Event
        var integrationEvent = new PaymentCapturedIntegrationEvent(
            domainEvent.PaymentId,
            domainEvent.OrderId,
            domainEvent.Amount,
            DateTime.UtcNow
        );

        // Publicar (salva no Outbox)
        await _eventBus.PublishAsync(integrationEvent, ct);
    }
}

// No módulo Orders (outro módulo!)
internal class PaymentCapturedIntegrationEventHandler
    : INotificationHandler<PaymentCapturedIntegrationEvent>
{
    public async Task Handle(PaymentCapturedIntegrationEvent @event, CancellationToken ct)
    {
        // Atualizar pedido
        var order = await _repository.GetByIdAsync(@event.OrderId);
        order.MarkAsPaid(@event.CapturedAt);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

---

### 4. **BaseEntityConfiguration**

Configuração base que mapeia convenções do PostgreSQL:

```csharp
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // ✅ Id → uuid PRIMARY KEY
        // ✅ Version → integer (se AggregateRoot)
        // ✅ CreatedAt/UpdatedAt → timestamptz (se IAuditableEntity)
        // ✅ DeletedAt → timestamptz (se ISoftDeletable)
        // ✅ Query Filter automático (WHERE deleted_at IS NULL)
        // ✅ DomainEvents ignorado (não persistido)
    }
}

// Uso:
public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");

        builder.Property(p => p.Sku)
            .HasColumnName("sku")
            .HasMaxLength(100);

        // ... outras propriedades
    }
}
```

---

### 5. **DateTimeProvider**

Abstração para testabilidade:

```csharp
// Produção
builder.Services.AddDateTimeProvider();
var now = _dateTimeProvider.UtcNow; // DateTime.UtcNow

// Testes
builder.Services.AddFakeDateTimeProvider(
    fixedDateTime: new DateTime(2025, 12, 13, 10, 0, 0)
);

var now = _dateTimeProvider.UtcNow; // Sempre retorna 2025-12-13 10:00:00
```

---

## 🚀 Configuração no DbContext

```csharp
public class CatalogDbContext : DbContext, IUnitOfWork
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 🎯 Registrar TODOS os interceptors
        optionsBuilder.AddInterceptors(
            new AuditableEntityInterceptor(_dateTimeProvider),
            new SoftDeleteInterceptor(_dateTimeProvider),
            new OptimisticConcurrencyInterceptor(),
            new PublishDomainEventsInterceptor("catalog") // Nome do módulo
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        // Aplicar configurações
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        // ⭐ IMPORTANTE: Registrar OutboxMessage
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    // IUnitOfWork
    public async Task<bool> SaveEntitiesAsync(CancellationToken ct = default)
    {
        try
        {
            await SaveChangesAsync(ct);
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

## 🎯 Alinhamento com Schema PostgreSQL

| Interceptor                      | PostgreSQL Equivalente                      |
| -------------------------------- | ------------------------------------------- |
| AuditableEntityInterceptor       | `DEFAULT NOW()` + `trigger_set_timestamp()` |
| SoftDeleteInterceptor            | Lógica de soft delete manual                |
| OptimisticConcurrencyInterceptor | `trigger_increment_version()`               |
| PublishDomainEventsInterceptor   | Sistema de eventos (não existe no SQL)      |

**IMPORTANTE**: Os interceptors funcionam **EM CONJUNTO** com os triggers do PostgreSQL, garantindo consistência mesmo se os triggers não estiverem ativos (útil para testes).

---

## 📊 Comparação: Antes vs Depois

### ❌ ANTES (Manual)

```csharp
public async Task AddAsync(Product product)
{
    // ❌ Manualmente definir timestamps
    product.CreatedAt = DateTime.UtcNow;
    product.UpdatedAt = DateTime.UtcNow;

    await _dbContext.Products.AddAsync(product);

    // ❌ Manualmente coletar eventos
    var events = product.DomainEvents.ToList();
    product.ClearDomainEvents();

    // ❌ Manualmente salvar no Outbox
    foreach (var @event in events)
    {
        var outboxMessage = new OutboxMessage { /* ... */ };
        await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
    }

    await _dbContext.SaveChangesAsync();
}
```

### ✅ DEPOIS (Com Interceptors)

```csharp
public async Task AddAsync(Product product)
{
    await _dbContext.Products.AddAsync(product);
    // ✅ TUDO automático:
    // - CreatedAt/UpdatedAt
    // - Domain Events → Outbox
    // - Version (se modificado)
}
```

---

## 📚 Arquitetura Completa Até Agora

```
BuildingBlocks/
├── Domain/                 ← Entidades, Value Objects, Events
│   └── ✅ Implementado
│
├── Application/            ← CQRS, Behaviors, Result Pattern
│   └── ✅ Implementado
│
└── Infrastructure/         ← EF Core, Interceptors, Outbox
    └── ✅ Implementado AGORA

Próximos:
└── [Modules]/             ← Implementar módulos do sistema
    ├── Catalog/
    ├── Orders/
    ├── Payments/
    └── ...
```

---

## ✅ Checklist de Qualidade

- [x] 4 Interceptors essenciais implementados
- [x] Outbox Pattern completo
- [x] OutboxProcessor com retry
- [x] BaseEntityConfiguration
- [x] EventBus para Integration Events
- [x] DateTimeProvider testável
- [x] Query Filters para soft delete
- [x] Optimistic Concurrency
- [x] Integração perfeita com schema PostgreSQL
- [x] Documentação completa
- [x] .NET 8 ready

---

## 🎓 Conceitos Implementados

1. **EF Core Interceptors**: Cross-cutting concerns
2. **Outbox Pattern**: Garantia de entrega de eventos
3. **Soft Delete**: Exclusão lógica com Query Filters
4. **Optimistic Concurrency**: Version-based locking
5. **Audit Trail**: Created/Updated timestamps
6. **Event Sourcing**: Domain Events → Outbox
7. **Integration Events**: Comunicação entre módulos
8. **Background Jobs**: OutboxProcessor
9. **Testability**: DateTimeProvider fake
10. **Convention over Configuration**: BaseEntityConfiguration

---

## 🚀 Próximos Passos

Agora você pode implementar os **DbContexts dos módulos**:

```csharp
// Catalog.Infrastructure
public class CatalogDbContext : DbContext, IUnitOfWork
{
    // Usar interceptors
    // Configurar entidades
    // Mapear OutboxMessage
}

// Orders.Infrastructure
public class OrdersDbContext : DbContext, IUnitOfWork
{
    // Mesma estrutura
}

// Etc...
```

Cada módulo terá:

- ✅ Seus próprios interceptors configurados
- ✅ Seu próprio Outbox (shared.domain_events)
- ✅ Seu próprio OutboxProcessor
- ✅ Communication via Integration Events

---

**Projeto**: E-commerce Modular Monolith  
**Versão**: 1.0.0  
**Data**: 2025-12-13  
**Status**: ✅ Implementação Completa

**Happy Coding! 🚀**
