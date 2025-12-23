using System.Reflection;
using Bcommerce.BuildingBlocks.Messaging.MassTransit.Configuration;
using Bcommerce.BuildingBlocks.Messaging.MassTransit.Filters;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.BuildingBlocks.Messaging.Extensions;

/// <summary>
/// Métodos de extensão para configuração do Message Broker (MassTransit).
/// </summary>
/// <remarks>
/// Configura o bus de mensagens da aplicação.
/// - Registra o MassTransit no container de DI
/// - Configura o transporte RabbitMQ
/// - Aplica convenções de nomenclatura (KebabCase) e filtros globais
/// 
/// Exemplo de uso:
/// <code>
/// builder.Services.AddMessageBroker(configuration, typeof(Program).Assembly);
/// </code>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona e configura o MassTransit com RabbitMQ.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="consumersAssembly">Assembly onde estão os consumidores (opcional).</param>
    /// <returns>A mesma coleção de serviços.</returns>
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
