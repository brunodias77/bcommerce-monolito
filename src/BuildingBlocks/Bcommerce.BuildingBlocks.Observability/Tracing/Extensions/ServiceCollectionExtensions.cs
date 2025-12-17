using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Bcommerce.BuildingBlocks.Observability.Tracing.Extensions;

/// <summary>
/// Extensões para configuração de tracing no DI.
/// </summary>
/// <remarks>
/// Inicializa o OpenTelemetry Tracing.
/// - Configura o provider de tracing com as instrumentações definidas em TracingConfiguration
/// - Permite que a aplicação participe de rastreios distribuídos
/// 
/// Exemplo de uso:
/// <code>
/// services.AddTracingServices("MyService");
/// </code>
/// </remarks>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTracingServices(this IServiceCollection services, string applicationName)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder => TracingConfiguration.ConfigureTracing(builder, applicationName));

        return services;
    }
}
