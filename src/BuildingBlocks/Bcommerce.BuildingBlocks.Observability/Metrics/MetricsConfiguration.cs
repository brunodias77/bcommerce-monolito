using OpenTelemetry.Metrics;

namespace Bcommerce.BuildingBlocks.Observability.Metrics;

/// <summary>
/// Configuração centralizada de métricas (OpenTelemetry).
/// </summary>
/// <remarks>
/// Define os medidores e instrumentações ativos.
/// - Adiciona instrumentação padrão (ASP.NET Core, HttpClient, Runtime)
/// - Configura medidor da aplicação para métricas customizadas
/// 
/// Exemplo de uso:
/// <code>
/// MetricsConfiguration.ConfigureMetrics(builder, "MyApplication");
/// </code>
/// </remarks>
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
