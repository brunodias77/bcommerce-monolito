using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Bcommerce.BuildingBlocks.Observability.Logging;

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
