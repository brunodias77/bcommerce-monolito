using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics.Extensions;

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
