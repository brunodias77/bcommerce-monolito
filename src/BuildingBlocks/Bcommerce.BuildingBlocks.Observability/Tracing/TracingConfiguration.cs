using OpenTelemetry.Trace;

namespace Bcommerce.BuildingBlocks.Observability.Tracing;

public static class TracingConfiguration
{
    public static void ConfigureTracing(TracerProviderBuilder builder, string applicationName)
    {
        builder
            .AddSource(applicationName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    }
}
