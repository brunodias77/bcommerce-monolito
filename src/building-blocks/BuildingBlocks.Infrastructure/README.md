# BuildingBlocks.Infrastructure

Componentes de infraestrutura compartilhados para o monolito modular BCommerce.

## Estrutura

```
BuildingBlocks.Infrastructure/
├── Persistence/
│   ├── Configurations/
│   │   └── BaseEntityConfiguration.cs
│   ├── Interceptors/
│   │   ├── AuditableEntityInterceptor.cs
│   │   ├── SoftDeleteInterceptor.cs
│   │   ├── PublishDomainEventsInterceptor.cs
│   │   └── OptimisticConcurrencyInterceptor.cs
│   ├── UnitOfWork.cs
│   └── UnitOfWorkExtensions.cs
├── Messaging/
│   └── Integration/
│       ├── IEventBus.cs
│       ├── InMemoryEventBus.cs
│       └── OutboxEventBus.cs
├── BackgroundJobs/
│   ├── ProcessOutboxMessagesJob.cs
│   └── CleanupExpiredSessionsJob.cs
├── Caching/
│   ├── ICacheService.cs
│   └── MemoryCacheService.cs
└── Services/
    ├── ICurrentUserService.cs
    ├── CurrentUserService.cs
    └── IDateTimeProvider.cs
```

## Uso

### Configuração no DI

```csharp
// Program.cs ou Startup.cs
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddHttpContextAccessor();
services.AddCurrentUserService();

// Interceptors (registrados por módulo)
services.AddSingleton<AuditableEntityInterceptor>();
services.AddSingleton<SoftDeleteInterceptor>();
services.AddSingleton(sp => new PublishDomainEventsInterceptor("users"));
services.AddSingleton<OptimisticConcurrencyInterceptor>();

// DbContext com interceptors
services.AddDbContext<UsersDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(
        sp.GetRequiredService<AuditableEntityInterceptor>(),
        sp.GetRequiredService<SoftDeleteInterceptor>(),
        // ...
    );
});

// Event Bus
services.AddScoped<IEventBus, InMemoryEventBus>();
// ou
services.AddScoped<IEventBus, OutboxEventBus>();

// Background Jobs
services.AddOutboxProcessor(options =>
{
    options.ProcessInterval = TimeSpan.FromSeconds(2);
    options.BatchSize = 20;
});

// Cache
services.AddMemoryCacheService(options =>
{
    options.DefaultExpiration = TimeSpan.FromMinutes(5);
});
```

### Interceptors

Os interceptors processam automaticamente durante `SaveChangesAsync`:

- **AuditableEntityInterceptor**: Preenche `CreatedAt` e `UpdatedAt`
- **SoftDeleteInterceptor**: Converte DELETE em UPDATE com `DeletedAt`
- **PublishDomainEventsInterceptor**: Salva events no Outbox
- **OptimisticConcurrencyInterceptor**: Loga conflitos de concorrência

### Cache

```csharp
// Injetar no construtor
private readonly ICacheService _cache;

// Usar
var user = await _cache.GetOrCreateAsync(
    $"user:{userId}",
    async () => await _repository.GetByIdAsync(userId),
    TimeSpan.FromMinutes(5));

// Invalidar
await _cache.RemoveAsync($"user:{userId}");
await _cache.RemoveByPrefixAsync("user:");
```

## Dependências

- Microsoft.EntityFrameworkCore 8.0
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0
- MediatR 12.2
- Newtonsoft.Json 13.0

## Referências

- [BuildingBlocks.Domain](../BuildingBlocks.Domain/README.md)
- [BuildingBlocks.Application](../BuildingBlocks.Application/README.md)
