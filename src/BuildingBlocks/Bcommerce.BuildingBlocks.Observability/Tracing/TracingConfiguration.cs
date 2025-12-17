using OpenTelemetry.Trace;

namespace Bcommerce.BuildingBlocks.Observability.Tracing;

/// <summary>
/// Configuração centralizada de rastreamento distribuído (Tracing).
/// </summary>
/// <remarks>
/// Configura fontes de dados e instrumentações para o OpenTelemetry Tracing.
/// - Instrumenta ASP.NET Core e HttpClient
/// - Adiciona a "Source" da aplicação para propagar contexto de rastreio
/// 
/// Exemplo de uso:
/// <code>
/// TracingConfiguration.ConfigureTracing(builder, "MyApplication");
/// </code>
/// </remarks>
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
