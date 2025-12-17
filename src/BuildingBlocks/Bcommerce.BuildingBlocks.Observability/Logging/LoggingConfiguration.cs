using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Bcommerce.BuildingBlocks.Observability.Logging;

/// <summary>
/// Configuração centralizada do Serilog para a aplicação.
/// </summary>
/// <remarks>
/// Define os sinks e enriquecedores de log padrão.
/// - Configura saída para Console
/// - Adiciona propriedades de contexto (Application, CorrelationId)
/// - Ajusta níveis de log baseados no ambiente (Debug/Info)
/// 
/// Exemplo de uso:
/// <code>
/// LoggingConfiguration.ConfigureLogging(builder.Host, "MyService");
/// </code>
/// </remarks>
public static class LoggingConfiguration
{
    public static void ConfigureLogging(IHostBuilder hostBuilder, string applicationName)
    {
        hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.With<SerilogEnrichers.CorrelationIdEnricher>()
                .WriteTo.Console();

            if (context.HostingEnvironment.IsDevelopment())
            {
                configuration.MinimumLevel.Debug();
            }
            else
            {
                configuration.MinimumLevel.Information();
            }
        });
    }
}
