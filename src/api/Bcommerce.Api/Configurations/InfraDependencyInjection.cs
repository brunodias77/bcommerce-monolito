using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Infrastructure.BackgroundJobs;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Messaging.Integration;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Api.Configurations;

/// <summary>
/// Configuração de Dependency Injection para a camada Infrastructure.
/// </summary>
/// <remarks>
/// Registra:
/// - Serviços compartilhados (DateTimeProvider, CurrentUserService)
/// - EF Core Interceptors
/// - Event Bus
/// - Cache Service
/// - Background Jobs
/// - DbContexts dos módulos
/// </remarks>
public static class InfraDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ===============================================================
        // Serviços Compartilhados
        // ===============================================================
        services.AddDateTimeProvider();
        services.AddHttpContextAccessor();
        services.AddCurrentUserService();

        // ===============================================================
        // EF Core Interceptors (Singletons para reutilização)
        // ===============================================================
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddSingleton<OptimisticConcurrencyInterceptor>();
        
        // PublishDomainEventsInterceptor é registrado por módulo com KeyedServices
        // Cada módulo tem sua própria instância identificada pelo nome do módulo
        services.AddKeyedSingleton("users", (sp, key) => new PublishDomainEventsInterceptor("users"));
        services.AddKeyedSingleton("catalog", (sp, key) => new PublishDomainEventsInterceptor("catalog"));
        services.AddKeyedSingleton("cart", (sp, key) => new PublishDomainEventsInterceptor("cart"));
        services.AddKeyedSingleton("orders", (sp, key) => new PublishDomainEventsInterceptor("orders"));
        services.AddKeyedSingleton("payments", (sp, key) => new PublishDomainEventsInterceptor("payments"));
        services.AddKeyedSingleton("coupons", (sp, key) => new PublishDomainEventsInterceptor("coupons"));

        // ===============================================================
        // Event Bus
        // ===============================================================
        // InMemoryEventBus para desenvolvimento/testes
        // Para produção, use OutboxEventBus + ProcessOutboxMessagesJob
        services.AddScoped<IEventBus, InMemoryEventBus>();

        // ===============================================================
        // Cache Service
        // ===============================================================
        services.AddMemoryCacheService(options =>
        {
            options.DefaultExpiration = TimeSpan.FromMinutes(5);
            options.KeyPrefix = "bcommerce";
        });

        // ===============================================================
        // Background Jobs (descomente para habilitar)
        // ===============================================================
        // Outbox Processor - processa domain events do Outbox
        // services.AddOutboxProcessor(options =>
        // {
        //     options.ProcessInterval = TimeSpan.FromSeconds(2);
        //     options.BatchSize = 20;
        //     options.MaxRetries = 3;
        // });

        // Session Cleanup - limpa sessões expiradas
        // services.AddSessionCleanupJob(options =>
        // {
        //     options.CleanupInterval = TimeSpan.FromMinutes(5);
        //     options.BatchSize = 100;
        // });

        // ===============================================================
        // Database Connection String
        // ===============================================================
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // ===============================================================
        // DbContexts dos Módulos
        // ===============================================================
        // Cada módulo tem seu próprio DbContext com interceptors compartilhados
        
        // Users Module - TODO: Descomentar quando Users.Infrastructure estiver pronto
        // services.AddDbContext<UsersDbContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
        //     });
        //     options.AddInterceptors(
        //         sp.GetRequiredService<AuditableEntityInterceptor>(),
        //         sp.GetRequiredService<SoftDeleteInterceptor>(),
        //         sp.GetRequiredKeyedService<PublishDomainEventsInterceptor>("users"),
        //         sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
        //     );
        // });
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

        // Catalog Module - TODO: Implementar CatalogDbContext
        // services.AddDbContext<CatalogDbContext>(...);

        // Orders Module - TODO: Implementar OrdersDbContext
        // services.AddDbContext<OrdersDbContext>(...);

        // Payments Module - TODO: Implementar PaymentsDbContext
        // services.AddDbContext<PaymentsDbContext>(...);

        // Cart Module - TODO: Implementar CartDbContext
        // services.AddDbContext<CartDbContext>(...);

        // Coupons Module - TODO: Implementar CouponsDbContext
        // services.AddDbContext<CouponsDbContext>(...);

        return services;
    }
}