using BuildingBlocks.Infrastructure.Messaging.Integration;
using Cart.Application.IntegrationEventHandlers;
using Cart.Core.Enums;
using Cart.Core.Repositories;
using Cart.Infrastructure.Persistence;
using Cart.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Users.Contracts.Events;

namespace Cart.Infrastructure;

/// <summary>
/// Extensão para registrar os serviços do módulo Cart.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCartModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ===============================================================
        // DbContext com mapeamento de ENUM PostgreSQL
        // ===============================================================
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Configura o DataSource com mapeamento do ENUM PostgreSQL
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<CartStatus>("shared.cart_status");
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<CartDbContext>(options =>
            options.UseNpgsql(
                dataSource,
                builder => builder.MigrationsHistoryTable("__EFMigrationsHistory", "cart")
            ));

        // ===============================================================
        // Repositories
        // ===============================================================
        services.AddScoped<ICartRepository, CartRepository>();

        // ===============================================================
        // Integration Event Handlers
        // ===============================================================
        // Registra o handler como tipo concreto (necessário para o InMemoryEventBus resolver)
        services.AddScoped<UserCreatedIntegrationEventHandler>();

        // Registra subscrição para o InMemoryEventBus
        InMemoryEventBus.RegisterHandler<UserCreatedIntegrationEvent, UserCreatedIntegrationEventHandler>();

        return services;
    }
}
