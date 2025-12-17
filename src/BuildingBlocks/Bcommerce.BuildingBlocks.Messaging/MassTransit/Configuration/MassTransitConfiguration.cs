using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Configuration;

/// <summary>
/// Configuração do MassTransit para uso com RabbitMQ.
/// </summary>
/// <remarks>
/// Configura o transporte de produção.
/// - Define host e credenciais do RabbitMQ
/// - Configura endpoints automaticamente
/// 
/// Exemplo de uso:
/// <code>
/// MassTransitConfiguration.ConfigureRabbitMq(context, cfg, configuration);
/// </code>
/// </remarks>
public static class MassTransitConfiguration
{
    public static void ConfigureRabbitMq(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RabbitMq");
        
        cfg.Host(connectionString ?? "amqp://guest:guest@localhost:5672");
        
        cfg.ConfigureEndpoints(context);
    }
}
