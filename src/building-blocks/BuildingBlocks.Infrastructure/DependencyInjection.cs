using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Extensões para configurar os serviços de infraestrutura compartilhados
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços de infraestrutura compartilhados ao container de DI
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registra o provedor de data/hora
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Registra os interceptors do EF Core
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<OutboxInterceptor>();
        services.AddScoped<InboxInterceptor>();

        return services;
    }

    /// <summary>
    /// Adiciona um DbContext com os interceptors padrão configurados
    /// </summary>
    /// <typeparam name="TContext">Tipo do DbContext</typeparam>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="connectionString">String de conexão com o banco de dados</param>
    /// <param name="configureOptions">Ação para configurar opções adicionais do DbContext</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddModuleDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            // Adiciona os interceptors padrão
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditInterceptor>(),
                serviceProvider.GetRequiredService<OutboxInterceptor>(),
                serviceProvider.GetRequiredService<InboxInterceptor>());

            // Configurações adicionais se fornecidas
            configureOptions?.Invoke(options);

            // Habilita logging sensível em desenvolvimento
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        return services;
    }
}
