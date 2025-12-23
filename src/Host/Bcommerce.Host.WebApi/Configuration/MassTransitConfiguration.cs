using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;

namespace Bcommerce.Host.WebApi.Configuration;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
            
            // If using RabbitMQ in the future, switch here based on config
        });

        return services;
    }
}
