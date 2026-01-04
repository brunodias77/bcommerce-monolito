using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace Web.API.Extensions;

/// <summary>
/// Extensões para configuração do Serilog (logging estruturado)
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configura o Serilog para a aplicação
    /// </summary>
    /// <param name="builder">WebApplicationBuilder</param>
    /// <returns>WebApplicationBuilder para encadeamento</returns>
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        // Remove o provider padrão de logging do ASP.NET Core
        builder.Logging.ClearProviders();

        // Configura Serilog a partir do appsettings.json
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Application", "BCommerce.API")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

        return builder;
    }

    /// <summary>
    /// Garante que logs sejam escritos antes do shutdown da aplicação
    /// </summary>
    /// <param name="app">WebApplication</param>
    public static void EnsureLogsAreFlushed(this WebApplication app)
    {
        // Registra callback para garantir flush dos logs no shutdown
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        lifetime.ApplicationStopping.Register(() =>
        {
            Log.Information("=== Encerrando BCommerce API ===");
        });

        lifetime.ApplicationStopped.Register(() =>
        {
            // Garante que todos os logs sejam escritos antes do shutdown
            Log.CloseAndFlush();
        });
    }
}
