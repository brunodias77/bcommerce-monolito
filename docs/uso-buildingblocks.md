# Análise e Mapeamento do BuildingBlocks - Arquitetura Monolito Modular

## 📊 Visão Geral da Arquitetura

```
BCommerce (Monolito Modular)
│
├── API Layer (Host)
│   ├── Program.cs
│   ├── Startup Configuration
│   └── Global Middleware Pipeline
│
├── Shared (BuildingBlocks)
│   ├── Domain
│   ├── Application
│   ├── Infrastructure
│   └── Presentation
│
└── Modules
    ├── Users
    ├── Catalog
    ├── Orders
    ├── Payments
    ├── Cart
    └── Coupons
```

---

## 🎯 BuildingBlocks.Domain

### **Onde Usa: MÓDULOS - Camada CORE**

| Componente | Localização | Uso |
|------------|-------------|-----|
| **Entity.cs** | `Modules.{Module}.Core/Entities` | Base para todas as entidades (Product, Order, User, etc.) |
| **AggregateRoot.cs** | `Modules.{Module}.Core/Aggregates` | Raízes de agregados (Order, Cart) com versionamento |
| **IAuditableEntity** | `Modules.{Module}.Core/Entities` | Interface para entidades auditáveis |
| **ISoftDeletable** | `Modules.{Module}.Core/Entities` | Interface para soft delete |
| **ValueObject.cs** | `Modules.{Module}.Core/ValueObjects` | Base para Address, Money, etc. |
| **Enumeration.cs** | `Modules.{Module}.Core/Enums` | Smart enums (OrderStatus, PaymentStatus) |
| **DomainEvent.cs** | `Modules.{Module}.Core/Events` | Eventos de domínio internos ao módulo |
| **IIntegrationEvent** | `Modules.{Module}.Contracts/Events` | Eventos entre módulos (contracts públicos) |
| **DomainException.cs** | `Modules.{Module}.Core/Exceptions` | Exceções de regras de negócio |
| **IRepository** | `Modules.{Module}.Core/Repositories` | Interfaces de repositório (abstrações) |
| **IUnitOfWork** | `Modules.{Module}.Core/Repositories` | Interface Unit of Work |

### Exemplo Prático:

```
Modules/
├── Catalog/
│   └── Catalog.Core/
│       ├── Products/
│       │   ├── Product.cs              (herda AggregateRoot, IAuditableEntity, ISoftDeletable)
│       │   ├── ProductImage.cs         (herda Entity)
│       │   └── ProductCreatedEvent.cs  (herda DomainEvent)
│       ├── Categories/
│       │   └── Category.cs
│       ├── ValueObjects/
│       │   └── ProductSku.cs           (herda ValueObject)
│       ├── Enums/
│       │   └── ProductStatus.cs        (herda Enumeration)
│       ├── Repositories/
│       │   └── IProductRepository.cs   (herda IRepository<Product>)
│       └── Exceptions/
│           └── InsufficientStockException.cs (herda DomainException)
```

---

## 🎯 BuildingBlocks.Application

### **Onde Usa: MÓDULOS - Camada APPLICATION**

| Componente | Localização | Uso |
|------------|-------------|-----|
| **ICommand / IQuery** | `Modules.{Module}.Application/Features` | Commands e Queries (CQRS) |
| **ICommandHandler / IQueryHandler** | `Modules.{Module}.Application/Features` | Handlers de Commands/Queries |
| **Result.cs / Error.cs** | `Modules.{Module}.Application` | Retorno de handlers sem exceções |
| **ValidationBehavior** | **API Layer** (global) | Pipeline MediatR para todos os módulos |
| **LoggingBehavior** | **API Layer** (global) | Pipeline MediatR para todos os módulos |
| **TransactionBehavior** | **API Layer** (global) | Pipeline MediatR para todos os módulos |
| **PagedResult / PaginationParams** | `Modules.{Module}.Application/DTOs` | Paginação em Queries |

### Exemplo Prático:

```
Modules/
├── Catalog/
│   └── Catalog.Application/
│       ├── Products/
│       │   ├── Commands/
│       │   │   ├── CreateProductCommand.cs              (: ICommand<Guid>)
│       │   │   ├── CreateProductCommandHandler.cs       (: ICommandHandler<CreateProductCommand, Guid>)
│       │   │   └── CreateProductCommandValidator.cs     (FluentValidation)
│       │   ├── Queries/
│       │   │   ├── GetProductByIdQuery.cs               (: IQuery<ProductDto>)
│       │   │   ├── GetProductByIdQueryHandler.cs        (: IQueryHandler<GetProductByIdQuery, ProductDto>)
│       │   │   └── SearchProductsQuery.cs               (: IQuery<PagedResult<ProductDto>>)
│       │   └── DTOs/
│       │       └── ProductDto.cs
│       └── DependencyInjection.cs
```

### Configuração Global (API Layer):

```csharp
// API/Program.cs
builder.Services.AddMediatR(cfg =>
{
    // Registra handlers de TODOS os módulos
    cfg.RegisterServicesFromAssembly(typeof(Catalog.Application.AssemblyMarker).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Orders.Application.AssemblyMarker).Assembly);
    // ...
});

// Behaviors globais (uma vez para todos os módulos)
builder.Services.AddLoggingBehavior();
builder.Services.AddValidationBehavior();
builder.Services.AddTransactionBehavior();
```

---

## 🎯 BuildingBlocks.Infrastructure

### **Onde Usa: MÓDULOS - Camada INFRASTRUCTURE + API Layer**

| Componente | Localização | Uso |
|------------|-------------|-----|
| **BaseEntityConfiguration** | `Modules.{Module}.Infrastructure/Persistence/Configurations` | Configurações EF Core |
| **AuditableEntityInterceptor** | **API Layer** (singleton global) | Interceptor compartilhado |
| **SoftDeleteInterceptor** | **API Layer** (singleton global) | Interceptor compartilhado |
| **PublishDomainEventsInterceptor** | `Modules.{Module}.Infrastructure` | Um por módulo (cada DbContext) |
| **OptimisticConcurrencyInterceptor** | **API Layer** (singleton global) | Interceptor compartilhado |
| **UnitOfWork / UnitOfWorkExtensions** | `Modules.{Module}.Infrastructure` | DbContext de cada módulo |
| **IEventBus (InMemory / Outbox)** | **API Layer** (singleton global) | Comunicação entre módulos |
| **ProcessOutboxMessagesJob** | **API Layer** | Background job global |
| **CleanupExpiredSessionsJob** | **API Layer** | Background job global |
| **ICacheService / MemoryCacheService** | **API Layer** (singleton global) | Cache compartilhado |
| **ICurrentUserService** | **API Layer** (scoped global) | Usuário autenticado |
| **IDateTimeProvider** | **API Layer** (singleton global) | Provedor de data/hora |

### Exemplo Prático:

#### 1. **Módulo - Infrastructure**

```
Modules/
├── Catalog/
│   └── Catalog.Infrastructure/
│       ├── Persistence/
│       │   ├── CatalogDbContext.cs                     (herda UnitOfWork, usa interceptors)
│       │   ├── Configurations/
│       │   │   ├── ProductConfiguration.cs             (herda BaseEntityConfiguration<Product>)
│       │   │   └── CategoryConfiguration.cs
│       │   └── Repositories/
│       │       └── ProductRepository.cs                (implementa IProductRepository)
│       └── DependencyInjection.cs
```

```csharp
// Catalog.Infrastructure/Persistence/CatalogDbContext.cs
public class CatalogDbContext : UnitOfWork
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}

// Catalog.Infrastructure/Persistence/Configurations/ProductConfiguration.cs
public class ProductConfiguration : SoftDeletableEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder); // Aplica configurações base

        builder.ToTable("products", "catalog");
        
        builder.Property(p => p.Sku).HasMaxLength(100);
        builder.Property(p => p.Name).HasMaxLength(150);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        
        builder.HasIndex(p => p.Sku).IsUnique();
    }
}
```

#### 2. **API Layer - Configuração Global**

```csharp
// API/Program.cs

// ===== 1. SERVIÇOS GLOBAIS =====
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCurrentUserService();
builder.Services.AddMemoryCacheService(options =>
{
    options.DefaultExpiration = TimeSpan.FromMinutes(5);
});

// ===== 2. INTERCEPTORS GLOBAIS (Singletons) =====
builder.Services.AddSingleton<AuditableEntityInterceptor>();
builder.Services.AddSingleton<SoftDeleteInterceptor>();
builder.Services.AddSingleton<OptimisticConcurrencyInterceptor>();

// ===== 3. DBCONTEXTS DOS MÓDULOS (cada um com seus interceptors) =====
// Catalog Module
builder.Services.AddDbContext<CatalogDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(
        sp.GetRequiredService<AuditableEntityInterceptor>(),
        sp.GetRequiredService<SoftDeleteInterceptor>(),
        new PublishDomainEventsInterceptor("catalog"), // Por módulo
        sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
    );
});

// Orders Module
builder.Services.AddDbContext<OrdersDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(
        sp.GetRequiredService<AuditableEntityInterceptor>(),
        sp.GetRequiredService<SoftDeleteInterceptor>(),
        new PublishDomainEventsInterceptor("orders"), // Por módulo
        sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
    );
});

// ===== 4. EVENT BUS GLOBAL =====
builder.Services.AddScoped<IEventBus, OutboxEventBus>();

// ===== 5. BACKGROUND JOBS GLOBAIS =====
builder.Services.AddOutboxProcessor(options =>
{
    options.ProcessInterval = TimeSpan.FromSeconds(2);
    options.BatchSize = 20;
});

builder.Services.AddSessionCleanupJob(options =>
{
    options.CleanupInterval = TimeSpan.FromMinutes(5);
});

// ===== 6. REGISTRAR MÓDULOS =====
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddOrdersModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
// ...
```

---

## 🎯 BuildingBlocks.Presentation

### **Onde Usa: API Layer + MÓDULOS - Camada PRESENTATION**

| Componente | Localização | Uso |
|------------|-------------|-----|
| **ApiControllerBase** | `Modules.{Module}.Presentation/Controllers` | Base para controllers de cada módulo |
| **ExceptionHandlingMiddleware** | **API Layer** (pipeline global) | Middleware global |
| **RequestLoggingMiddleware** | **API Layer** (pipeline global) | Middleware global |
| **ExceptionHandlingFilter** | **API Layer** (filtro global) | Filtro MVC global |
| **ValidationFilter** | **API Layer** (filtro global) | Filtro MVC global |
| **ResultExtensions** | `Modules.{Module}.Presentation` | Extensões usadas nos controllers |
| **ProblemDetailsExtensions** | `Modules.{Module}.Presentation` | Extensões usadas nos controllers |

### Exemplo Prático:

#### 1. **Módulo - Presentation**

```
Modules/
├── Catalog/
│   └── Catalog.Presentation/
│       ├── Controllers/
│       │   ├── ProductsController.cs        (herda ApiControllerBase)
│       │   └── CategoriesController.cs
│       └── DependencyInjection.cs
```

```csharp
// Catalog.Presentation/Controllers/ProductsController.cs
[Route("api/catalog/products")]
public class ProductsController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query);
        
        return HandleResult(result); // Helper do ApiControllerBase
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var command = request.ToCommand();
        var result = await Mediator.Send(command);
        
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Value });
    }
}
```

#### 2. **API Layer - Configuração Global**

```csharp
// API/Program.cs

// ===== 1. CONTROLLERS + FILTROS GLOBAIS =====
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionHandlingFilter>();
    options.Filters.Add<ValidationFilter>();
})
.AddApplicationPart(typeof(Catalog.Presentation.AssemblyMarker).Assembly)
.AddApplicationPart(typeof(Orders.Presentation.AssemblyMarker).Assembly)
.AddApplicationPart(typeof(Users.Presentation.AssemblyMarker).Assembly);
// ...

// ===== 2. PIPELINE DE MIDDLEWARES =====
var app = builder.Build();

app.UseExceptionHandlingMiddleware();    // 1. Primeiro - captura exceções
app.UseRequestLoggingMiddleware();       // 2. Segundo - logging

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Mapeia controllers de TODOS os módulos
```

---

## 📋 Resumo: BuildingBlocks por Camada

### **API Layer (Host)**

✅ **Configurações Globais:**
- MediatR Behaviors (Validation, Logging, Transaction)
- Interceptors (Auditable, SoftDelete, OptimisticConcurrency)
- Middlewares (Exception, RequestLogging)
- Filtros MVC (ExceptionHandling, Validation)
- Serviços Globais (CurrentUser, DateTimeProvider, Cache, EventBus)
- Background Jobs (OutboxProcessor, SessionCleanup)
- DbContexts de todos os módulos

### **Módulos - Core (Domain)**

✅ **BuildingBlocks.Domain:**
- Entity, AggregateRoot
- IAuditableEntity, ISoftDeletable
- ValueObject, Enumeration
- DomainEvent, IIntegrationEvent
- DomainException
- IRepository, IUnitOfWork (interfaces)

### **Módulos - Application**

✅ **BuildingBlocks.Application:**
- ICommand, IQuery
- ICommandHandler, IQueryHandler
- Result, Error
- PagedResult, PaginationParams

### **Módulos - Infrastructure**

✅ **BuildingBlocks.Infrastructure:**
- BaseEntityConfiguration (EF Core)
- PublishDomainEventsInterceptor (por módulo)
- UnitOfWork, UnitOfWorkExtensions
- Implementação de Repositórios

### **Módulos - Presentation**

✅ **BuildingBlocks.Presentation:**
- ApiControllerBase
- ResultExtensions, ProblemDetailsExtensions

### **Módulos - Contracts (Público)**

✅ **BuildingBlocks.Domain:**
- IIntegrationEvent
- IntegrationEvent

---

## 🎯 Diagrama de Dependências

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  ┌────────────────────────────────────────────────────┐    │
│  │ Program.cs                                          │    │
│  │ - MediatR Behaviors (Global)                       │    │
│  │ - Interceptors (Global)                            │    │
│  │ - Middlewares (Global)                             │    │
│  │ - Filters (Global)                                 │    │
│  │ - Services (CurrentUser, Cache, EventBus, etc.)    │    │
│  │ - Background Jobs                                  │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                      BuildingBlocks                         │
│  ┌──────────────┬──────────────┬──────────────┬─────────┐  │
│  │   Domain     │ Application  │Infrastructure│ Present.│  │
│  └──────────────┴──────────────┴──────────────┴─────────┘  │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                         Modules                             │
│  ┌────────────────┐  ┌────────────────┐  ┌──────────────┐  │
│  │    Catalog     │  │     Orders     │  │    Users     │  │
│  │  ┌──────────┐  │  │  ┌──────────┐  │  │  ┌────────┐  │  │
│  │  │Core      │  │  │  │Core      │  │  │  │Core    │  │  │
│  │  │Application│  │  │  │Application│  │  │  │App.   │  │  │
│  │  │Infra.    │  │  │  │Infra.    │  │  │  │Infra.  │  │  │
│  │  │Present.  │  │  │  │Present.  │  │  │  │Present.│  │  │
│  │  │Contracts │  │  │  │Contracts │  │  │  │Contract│  │  │
│  │  └──────────┘  │  │  └──────────┘  │  │  └────────┘  │  │
│  └────────────────┘  └────────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## ✅ Checklist de Implementação

### **Fase 1: API Layer**
- [ ] Configurar MediatR com Behaviors globais
- [ ] Registrar Interceptors globais
- [ ] Configurar Middlewares (Exception, Logging)
- [ ] Configurar Filtros MVC
- [ ] Configurar serviços globais (CurrentUser, Cache, EventBus)
- [ ] Configurar Background Jobs

### **Fase 2: Por Módulo**
- [ ] **Core:** Criar entidades herdando de Entity/AggregateRoot
- [ ] **Core:** Criar Value Objects, Enumerations, Domain Events
- [ ] **Core:** Definir interfaces de repositórios
- [ ] **Application:** Criar Commands/Queries e Handlers
- [ ] **Application:** Criar Validators (FluentValidation)
- [ ] **Application:** Criar DTOs
- [ ] **Infrastructure:** Criar DbContext herdando de UnitOfWork
- [ ] **Infrastructure:** Criar Configurations (EF Core)
- [ ] **Infrastructure:** Implementar Repositórios
- [ ] **Presentation:** Criar Controllers herdando de ApiControllerBase
- [ ] **Contracts:** Publicar Integration Events

### **Fase 3: Integração**
- [ ] Registrar DbContexts no API Layer
- [ ] Registrar Handlers do módulo no MediatR
- [ ] Registrar Controllers do módulo
- [ ] Configurar assinaturas de Integration Events no EventBus

---

**Versão:** 1.0.0  
**Data:** 2025-12-14  
**Projeto:** BCommerce - Monolito Modular