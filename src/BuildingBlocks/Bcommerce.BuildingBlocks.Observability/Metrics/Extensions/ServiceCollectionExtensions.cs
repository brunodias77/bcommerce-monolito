using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics.Extensions;

/// <summary>
/// Extensões para configuração de métricas no DI.
/// </summary>
/// <remarks>
/// Inicializa o OpenTelemetry Metrics e registra classes de métricas customizadas.
/// - Configura provedor de métricas
/// - Registra BusinessMetrics e PerformanceMetrics como Singletons
/// 
/// Exemplo de uso:
/// <code>
/// services.AddMetricsServices("MyService");
/// </code>
/// </remarks>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsServices(this IServiceCollection services, string applicationName)
    {
        services.AddOpenTelemetry()
            .WithMetrics(builder => MetricsConfiguration.ConfigureMetrics(builder, applicationName));

        services.AddSingleton(new CustomMetrics.BusinessMetrics(applicationName));
        services.AddSingleton(new CustomMetrics.PerformanceMetrics(applicationName));

        return services;
    }
}
