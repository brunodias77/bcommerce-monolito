using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Infrastructure.BackgroundJobs;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Messaging.Integration;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Infrastructure.Services;
using Cart.Infrastructure;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure;

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
        // SERVIÇOS COMPARTILHADOS (SHARED KERNEL)
        // ===============================================================
        // Serviços utilitários usados por toda a aplicação.
        services.AddDateTimeProvider();      // IDateTimeProvider (facilita testes de tempo)
        services.AddHttpContextAccessor();   // Acesso ao HttpContext atual
        services.AddCurrentUserService();    // ICurrentUserService (identifica o usuário logado)

        // ===============================================================
        // EF CORE INTERCEPTORS
        // ===============================================================
        // Interceptam operações de SaveChanges para aplicar lógica transversal.
        // Registrados como Singleton pois não mantêm estado.
        
        services.AddSingleton<AuditableEntityInterceptor>(); // Preenche CreatedAt/UpdatedAt
        services.AddSingleton<SoftDeleteInterceptor>();      // Transforma Delete em Soft Delete (DeletedAt)
        services.AddSingleton<OptimisticConcurrencyInterceptor>(); // Trata concorrência otimista (Version)

        // PublishDomainEventsInterceptor: Transforma Domain Events em Mensagens de Outbox.
        // Registrado via Keyed Services porque cada módulo precisa de uma instância com o nome do seu Schema.
        services.AddKeyedSingleton("users", (sp, key) => new PublishDomainEventsInterceptor("users"));
        services.AddKeyedSingleton("catalog", (sp, key) => new PublishDomainEventsInterceptor("catalog"));
        services.AddKeyedSingleton("cart", (sp, key) => new PublishDomainEventsInterceptor("cart"));
        services.AddKeyedSingleton("orders", (sp, key) => new PublishDomainEventsInterceptor("orders"));
        services.AddKeyedSingleton("payments", (sp, key) => new PublishDomainEventsInterceptor("payments"));
        services.AddKeyedSingleton("coupons", (sp, key) => new PublishDomainEventsInterceptor("coupons"));

        // ===============================================================
        // EVENT BUS (Mensageria)
        // ===============================================================
        // Responsável por enviar Integation Events entre módulos.
        // - InMemoryEventBus: Síncrono, ideal para dev/testes, mas sem resiliência.
        // - OutboxEventBus (Futuro): Assíncrono, grava no banco antes de enviar (RabbitMQ/Azure).
        services.AddScoped<IEventBus, InMemoryEventBus>();

        // ===============================================================
        // CACHE DISTRIBUÍDO
        // ===============================================================
        // Abstração de cache (hoje MemoryCache, futuramente Redis).
        services.AddMemoryCacheService(options =>
        {
            options.DefaultExpiration = TimeSpan.FromMinutes(5);
            options.KeyPrefix = "bcommerce";
        });

        // ===============================================================
        // BACKGROUND JOBS (Tarefas em Segundo Plano)
        // ===============================================================
        // Processadores que rodam em background (HostedServices).
        
        // Outbox Processor: Lê a tabela outbox_messages e publica os eventos pendentes.
        // Atualmente configurado apenas para o CatalogDbContext.
        // TODO: Configurar para outros módulos (Users, Orders) quando tiverem Outbox implementado.
        services.AddOutboxProcessor<Catalog.Infrastructure.Persistence.CatalogDbContext>(options =>
        {
            options.ProcessInterval = TimeSpan.FromSeconds(2);
            options.BatchSize = 20;
            options.MaxRetries = 3;
        });

        // Outbox Processor para Users
        services.AddOutboxProcessor<Users.Infrastructure.Persistence.UsersDbContext>(options =>
        {
            options.ProcessInterval = TimeSpan.FromSeconds(2);
            options.BatchSize = 20;
            options.MaxRetries = 3;
        });

        // ===============================================================
        // DATABASE & MODULES
        // ===============================================================
        
        // Connection String principal
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Configuração de cada módulo (DbContexts, Repositories, etc.)
        // Cada módulo é responsável por registrar suas próprias dependências.

        services.AddUsersModule(configuration);   // Módulo de Usuários
        services.AddCatalogModule(configuration); // Módulo de Catálogo
        services.AddCartModule(configuration);    // Módulo de Carrinho

        // TODO: Implementar e descomentar conforme novos módulos surgirem
        // services.AddOrdersModule(configuration);
        // services.AddPaymentsModule(configuration);

        return services;
    }
}