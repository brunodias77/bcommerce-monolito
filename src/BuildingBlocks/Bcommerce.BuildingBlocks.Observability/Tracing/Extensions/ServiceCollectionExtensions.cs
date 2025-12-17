using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Bcommerce.BuildingBlocks.Observability.Tracing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTracingServices(this IServiceCollection services, string applicationName)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder => TracingConfiguration.ConfigureTracing(builder, applicationName));

        return services;
    }
}
