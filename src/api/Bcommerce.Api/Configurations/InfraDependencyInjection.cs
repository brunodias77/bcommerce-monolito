using BuildingBlocks.Infrastructure.Messaging.Integration;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ===== SERVIÇOS DE INFRAESTRUTURA COMPARTILHADOS =====

        // DateTime Provider - Facilita testes e garante consistência de timezone
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // HttpContextAccessor - Necessário para CurrentUserService
        services.AddHttpContextAccessor();

        // Current User Service - Acessa informações do usuário autenticado
        services.AddCurrentUserService();

        // Event Bus - Para publicação de Integration Events entre módulos
        services.AddScoped<IEventBus, EventBus>();

        // ===== CONFIGURAÇÃO DE INTERCEPTORS (EF CORE) =====
        // Os interceptors são registrados por módulo, cada um com seu nome específico

        // Auditable Entity Interceptor - Preenche CreatedAt e UpdatedAt automaticamente
        services.AddSingleton<AuditableEntityInterceptor>();

        // Soft Delete Interceptor - Converte DELETE em UPDATE (soft delete)
        services.AddSingleton<SoftDeleteInterceptor>();

        // Publish Domain Events Interceptor - Salva eventos no Outbox durante SaveChanges
        // Cada módulo precisa de seu próprio interceptor com nome específico
        services.AddSingleton(sp => new PublishDomainEventsInterceptor("users"));
        services.AddSingleton(sp => new PublishDomainEventsInterceptor("catalog"));
        services.AddSingleton(sp => new PublishDomainEventsInterceptor("orders"));
        services.AddSingleton(sp => new PublishDomainEventsInterceptor("payments"));
        services.AddSingleton(sp => new PublishDomainEventsInterceptor("coupons"));
        services.AddSingleton(sp => new PublishDomainEventsInterceptor("cart"));

        // Optimistic Concurrency Interceptor - Gerencia concorrência otimista
        services.AddSingleton<OptimisticConcurrencyInterceptor>();

        // ===== CONFIGURAÇÃO DOS DbCONTEXTS E UNIT OF WORK DE CADA MÓDULO =====

        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' not found.");

        // ----- MÓDULO: USERS -----
        // services.AddDbContext<UsersDbContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "users");
        //         npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //     });
        //
        //     // Adiciona os interceptors
        //     options.AddInterceptors(
        //         sp.GetRequiredService<AuditableEntityInterceptor>(),
        //         sp.GetRequiredService<SoftDeleteInterceptor>(),
        //         sp.GetServices<PublishDomainEventsInterceptor>()
        //             .First(i => i.ModuleName == "users"),
        //         sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
        //     );
        //
        //     // Configurações adicionais
        //     options.EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"));
        //     options.EnableDetailedErrors(configuration.GetValue<bool>("Logging:EnableDetailedErrors"));
        // });
        //
        // // Registra o UnitOfWork do módulo Users
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

        // ----- MÓDULO: CATALOG -----
        // services.AddDbContext<CatalogDbContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "catalog");
        //         npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //     });
        //
        //     options.AddInterceptors(
        //         sp.GetRequiredService<AuditableEntityInterceptor>(),
        //         sp.GetRequiredService<SoftDeleteInterceptor>(),
        //         sp.GetServices<PublishDomainEventsInterceptor>()
        //             .First(i => i.ModuleName == "catalog"),
        //         sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
        //     );
        //
        //     options.EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"));
        //     options.EnableDetailedErrors(configuration.GetValue<bool>("Logging:EnableDetailedErrors"));
        // });
        //
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());

        // ----- MÓDULO: ORDERS -----
        // services.AddDbContext<OrdersDbContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "orders");
        //         npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //     });
        //
        //     options.AddInterceptors(
        //         sp.GetRequiredService<AuditableEntityInterceptor>(),
        //         sp.GetRequiredService<SoftDeleteInterceptor>(),
        //         sp.GetServices<PublishDomainEventsInterceptor>()
        //             .First(i => i.ModuleName == "orders"),
        //         sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
        //     );
        //
        //     options.EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"));
        //     options.EnableDetailedErrors(configuration.GetValue<bool>("Logging:EnableDetailedErrors"));
        // });
        //
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrdersDbContext>());

        // ----- MÓDULO: PAYMENTS -----
        // services.AddDbContext<PaymentsDbContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "payments");
        //         npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //     });
        //
        //     options.AddInterceptors(
        //         sp.GetRequiredService<AuditableEntityInterceptor>(),
        //         sp.GetRequiredService<SoftDeleteInterceptor>(),
        //         sp.GetServices<PublishDomainEventsInterceptor>()
        //             .First(i => i.ModuleName == "payments"),
        //         sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
        //     );
        //
        //     options.EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"));
        //     options.EnableDetailedErrors(configuration.GetValue<bool>("Logging:EnableDetailedErrors"));
        // });
        //
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());

        // ----- MÓDULO: COUPONS -----
        // services.AddDbContext<CouponsDbContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "coupons");
        //         npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //     });
        //
        //     options.AddInterceptors(
        //         sp.GetRequiredService<AuditableEntityInterceptor>(),
        //         sp.GetRequiredService<SoftDeleteInterceptor>(),
        //         sp.GetServices<PublishDomainEventsInterceptor>()
        //             .First(i => i.ModuleName == "coupons"),
        //         sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
        //     );
        //
        //     options.EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"));
        //     options.EnableDetailedErrors(configuration.GetValue<bool>("Logging:EnableDetailedErrors"));
        // });
        //
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CouponsDbContext>());

        // ----- MÓDULO: CART -----
        // services.AddDbContext<CartDbContext>((sp, options) =>
        // {
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "cart");
        //         npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //     });
        //
        //     options.AddInterceptors(
        //         sp.GetRequiredService<AuditableEntityInterceptor>(),
        //         sp.GetRequiredService<SoftDeleteInterceptor>(),
        //         sp.GetServices<PublishDomainEventsInterceptor>()
        //             .First(i => i.ModuleName == "cart"),
        //         sp.GetRequiredService<OptimisticConcurrencyInterceptor>()
        //     );
        //
        //     options.EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"));
        //     options.EnableDetailedErrors(configuration.GetValue<bool>("Logging:EnableDetailedErrors"));
        // });
        //
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CartDbContext>());

        // ===== OUTBOX PROCESSOR (BACKGROUND SERVICE) =====
        // Processa eventos do Outbox (shared.domain_events) e publica para outros módulos
        // services.AddHostedService<OutboxProcessor>();

        // ===== REPOSITÓRIOS =====
        // Cada módulo registra seus próprios repositórios
        // Exemplo:
        // services.AddScoped<IProductRepository, ProductRepository>();
        // services.AddScoped<IOrderRepository, OrderRepository>();
        // etc...
    }
}