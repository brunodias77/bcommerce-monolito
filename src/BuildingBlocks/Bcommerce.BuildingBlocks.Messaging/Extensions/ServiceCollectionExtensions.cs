using System.Reflection;
using Bcommerce.BuildingBlocks.Messaging.MassTransit.Configuration;
using Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.BuildingBlocks.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration, Assembly? consumersAssembly = null)
    {
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            if (consumersAssembly != null)
            {
                busConfigurator.AddConsumers(consumersAssembly);
            }

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                // Configuração padrão
                var connectionString = configuration.GetConnectionString("RabbitMq");
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback para InMemory se Rabbit não configurado (dev/test)
                    // Mas o ideal é lançar erro em prod.
                    // Para simplificar, configuramos Rabbit.
                }

                configurator.Host(connectionString ?? "amqp://guest:guest@localhost:5672");
                
                // Filtros Globais
                configurator.UseConsumeFilter(typeof(LoggingFilter<>), context);
                configurator.UseConsumeFilter(typeof(ExceptionHandlingFilter<>), context);

                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
