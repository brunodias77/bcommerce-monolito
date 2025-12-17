using OpenTelemetry.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics;

public static class MetricsConfiguration
{
    public static void ConfigureMetrics(MeterProviderBuilder builder, string applicationName)
    {
        builder
            .AddMeter(applicationName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
    }
}
