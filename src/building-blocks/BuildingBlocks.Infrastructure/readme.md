# BuildingBlocks.Infrastructure

Biblioteca de blocos de construção para a **Infrastructure Layer** com **EF Core Interceptors**, **Outbox Pattern** e integração com o schema PostgreSQL.

---

## 📦 Estrutura

```
BuildingBlocks.Infrastructure/
├── Persistence/
│   ├── Configurations/
│   │   └── BaseEntityConfiguration.cs     # Configuração base para entidades
│   │
│   └── Interceptors/                      # 🎯 INTERCEPTORS - A "Mágica"
│       ├── AuditableEntityInterceptor.cs  # created_at, updated_at
│       ├── SoftDeleteInterceptor.cs       # Soft delete (deleted_at)
│       ├── OptimisticConcurrencyInterceptor.cs # Version (Optimistic Locking)
│       └── PublishDomainEventsInterceptor.cs   # Outbox Pattern
│
├── Messaging/
│   ├── Outbox/
│   │   ├── OutboxMessage.cs               # Mapeia shared.domain_events
│   │   └── OutboxProcessor.cs             # Background job
│   │
│   └── Integration/
│       ├── IEventBus.cs                   # Interface para Integration Events
│       └── EventBus.cs                    # Implementação com Outbox
│
└── Services/
    └── DateTimeProvider.cs                # Testabilidade de datas
```

---

## 🎯 Interceptors - Integração com PostgreSQL

Os **Interceptors** são o coração desta biblioteca. Eles conectam o EF Core com os triggers e convenções do seu schema PostgreSQL.

### 1. AuditableEntityInterceptor

Preenche automaticamente `created_at` e `updated_at`:

```csharp
// Entidade
public class Product : AggregateRoot, IAuditableEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
}

// Ao fazer SaveChangesAsync():
// - Created: CreatedAt = NOW(), UpdatedAt = NOW()
// - Modified: UpdatedAt = NOW() (CreatedAt não muda)
```

**Trabalha EM CONJUNTO com triggers PostgreSQL:**

```sql
-- Schema PostgreSQL
created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()

CREATE TRIGGER trg_products_updated_at
    BEFORE UPDATE ON catalog.products
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_set_timestamp();
```

### 2. SoftDeleteInterceptor

Converte `DELETE` em `UPDATE` (soft delete):

```csharp
// Código C#
dbContext.Products.Remove(product);
await dbContext.SaveChangesAsync();

// SQL gerado:
// UPDATE catalog.products SET deleted_at = NOW() WHERE id = '...'
// (ao invés de DELETE)
```

**Query Filter automático:**

```csharp
// Queries automáticas ignoram deletados
var products = await dbContext.Products.ToListAsync();
// SELECT * FROM products WHERE deleted_at IS NULL

// Incluir deletados explicitamente
var allProducts = await dbContext.Products
    .IncludeDeleted()
    .ToListAsync();
```

### 3. OptimisticConcurrencyInterceptor

Incrementa `version` automaticamente para Optimistic Concurrency:

```csharp
// Thread A e B leem o mesmo Order (version=1)
var order = await dbContext.Orders.FindAsync(orderId);

// Thread A salva primeiro
order.AddItem(...);
await dbContext.SaveChangesAsync();
// → version=2 ✓

// Thread B tenta salvar
await dbContext.SaveChangesAsync();
// → DbUpdateConcurrencyException! (version esperada=1, real=2)
```

**Trabalha com trigger PostgreSQL:**

```sql
CREATE TRIGGER trg_orders_version
    BEFORE UPDATE ON orders.orders
    FOR EACH ROW EXECUTE FUNCTION shared.trigger_increment_version();
```

### 4. PublishDomainEventsInterceptor ⭐

O mais importante! Salva Domain Events no Outbox automaticamente:

```csharp
// 1. Entidade levanta evento
var product = Product.Create("SKU-001", "Product", 99.90m, 10);
// product.DomainEvents contém ProductCreatedEvent

// 2. Salvar
await _repository.AddAsync(product);
await _unitOfWork.SaveChangesAsync();

// 3. Interceptor automaticamente:
//    - Coleta todos os Domain Events
//    - Serializa como JSON
//    - Salva em shared.domain_events (Outbox)
//    - Limpa eventos da entidade

// 4. Background job (OutboxProcessor) processa depois
```

---

## 📬 Outbox Pattern

### Fluxo Completo

```
1. Domain Event levantado
       ↓
2. SaveChangesAsync()
       ↓
3. PublishDomainEventsInterceptor
       ├─ Serializa evento
       └─ Salva em shared.domain_events
       ↓
4. Commit da transação (evento + dados)
       ↓
5. OutboxProcessor (background job)
       ├─ Busca eventos não processados
       ├─ Deserializa
       ├─ Publica via MediatR
       └─ Marca como processado
       ↓
6. Handlers recebem evento
```

### OutboxMessage

Mapeia a tabela `shared.domain_events` do PostgreSQL:

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Module { get; set; }              // "catalog", "orders", etc.
    public string AggregateType { get; set; }       // "Product", "Order"
    public Guid AggregateId { get; set; }
    public string EventType { get; set; }           // "ProductCreatedEvent"
    public string Payload { get; set; }             // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
```

### OutboxProcessor

Background service que processa eventos:

```csharp
// Configuração
builder.Services.AddOutboxProcessor(
    interval: TimeSpan.FromSeconds(5),
    batchSize: 100,
    maxRetries: 3
);

// O que faz:
// 1. Roda a cada 5 segundos
// 2. Busca 100 eventos não processados
// 3. Publica via MediatR
// 4. Marca como processado ou incrementa retry_count
// 5. Se retry_count >= 3, marca como falho
```

---

## 🔄 Integration Events

### IEventBus

Interface para publicar eventos entre módulos:

```csharp
// No módulo Payments
internal class PaymentCapturedEventHandler
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
            DateTime.UtcNow
        );

        // Publicar (salva no Outbox)
        await _eventBus.PublishAsync(integrationEvent, ct);
    }
}

// No módulo Orders
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

## 🚀 Configuração Completa

### 1. DbContext do Módulo

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

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Registrar interceptors
        optionsBuilder
            .AddInterceptors(
                new AuditableEntityInterceptor(_dateTimeProvider),
                new SoftDeleteInterceptor(_dateTimeProvider),
                new OptimisticConcurrencyInterceptor(),
                new PublishDomainEventsInterceptor("catalog")
            );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar schema
        modelBuilder.HasDefaultSchema("catalog");

        // Aplicar configurações
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        // Configurar OutboxMessage
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    // IUnitOfWork
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

### 2. Configuração de Entidade

```csharp
public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");

        builder.Property(p => p.Sku)
            .HasColumnName("sku")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.Stock)
            .HasColumnName("stock")
            .HasColumnType("integer")
            .IsRequired();

        // Relacionamentos
        builder.HasMany<ProductImage>()
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 3. Startup/Program.cs

```csharp
// DbContext
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
            npgsqlOptions.EnableRetryOnFailure(3);
        });
});

// Services
builder.Services.AddDateTimeProvider();
builder.Services.AddEventBus("catalog");
builder.Services.AddOutboxProcessor(
    interval: TimeSpan.FromSeconds(5),
    batchSize: 100,
    maxRetries: 3
);

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
```

---

## 📊 Comparação: Com vs Sem Interceptors

### ❌ SEM Interceptors (Manual)

```csharp
public class ProductRepository : IProductRepository
{
    public async Task AddAsync(Product product)
    {
        // Manualmente definir timestamps
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.Products.AddAsync(product);

        // Manualmente coletar eventos
        var events = product.DomainEvents.ToList();
        product.ClearDomainEvents();

        // Manualmente salvar no Outbox
        foreach (var @event in events)
        {
            var outboxMessage = new OutboxMessage { /* ... */ };
            await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        // Manualmente atualizar timestamp
        product.UpdatedAt = DateTime.UtcNow;

        // Manualmente incrementar version
        product.IncrementVersion();

        _dbContext.Products.Update(product);
        // ... repetir lógica de eventos
    }

    public async Task RemoveAsync(Product product)
    {
        // Manualmente fazer soft delete
        product.DeletedAt = DateTime.UtcNow;
        _dbContext.Products.Update(product);
    }
}
```

### ✅ COM Interceptors (Automático)

```csharp
public class ProductRepository : IProductRepository
{
    public async Task AddAsync(Product product)
    {
        await _dbContext.Products.AddAsync(product);
        // ✅ Interceptors fazem TUDO automaticamente:
        // - CreatedAt/UpdatedAt
        // - Domain Events → Outbox
        // - Version
    }

    public async Task UpdateAsync(Product product)
    {
        _dbContext.Products.Update(product);
        // ✅ Interceptors atualizam tudo
    }

    public async Task RemoveAsync(Product product)
    {
        _dbContext.Products.Remove(product);
        // ✅ Interceptor converte DELETE → soft delete
    }
}
```

---

## 🎓 Boas Práticas

### ✅ DO:

- Use interceptors para cross-cutting concerns
- Configure OutboxProcessor como singleton
- Use IDateTimeProvider para testabilidade
- Aproveite Query Filters para soft delete
- Configure retry no OutboxProcessor
- Use BaseEntityConfiguration para entidades

### ❌ DON'T:

- Não rode múltiplos OutboxProcessors sem distributed lock
- Não ignore DbUpdateConcurrencyException
- Não force hard delete sem necessidade
- Não desabilite Query Filters globalmente
- Não misture lógica de negócio em interceptors

---

## 📖 Próximos Passos

Com BuildingBlocks.Infrastructure, você está pronto para:

1. **Implementar DbContexts dos módulos**:

   - CatalogDbContext
   - OrdersDbContext
   - PaymentsDbContext

2. **Criar Configurations EF Core**:

   - ProductConfiguration
   - OrderConfiguration
   - PaymentConfiguration

3. **Implementar Repositories**:
   - Usar UnitOfWork
   - Aproveitar interceptors

---

**Versão**: 1.0.0  
**Data**: 2025-12-13  
**Projeto**: E-commerce Modular Monolith
