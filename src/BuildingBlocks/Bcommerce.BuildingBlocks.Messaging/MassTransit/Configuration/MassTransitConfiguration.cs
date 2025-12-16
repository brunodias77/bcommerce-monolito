using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Bcommerce.BuildingBlocks.Messaging.MassTransit.Configuration;

public static class MassTransitConfiguration
{
    public static void ConfigureRabbitMq(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RabbitMq");
        
        cfg.Host(connectionString ?? "amqp://guest:guest@localhost:5672");
        
        cfg.ConfigureEndpoints(context);
    }
}
