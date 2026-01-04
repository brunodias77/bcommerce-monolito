using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace Web.API.Extensions;

/// <summary>
/// Extensões para configuração do Serilog (logging estruturado)
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Tema customizado com as cores do Dracula Theme
    /// </summary>
    public static AnsiConsoleTheme DraculaTheme { get; } = new AnsiConsoleTheme(
        new Dictionary<ConsoleThemeStyle, string>
        {
            [ConsoleThemeStyle.Text] = "\x1b[38;2;248;248;242m",           // Foreground
            [ConsoleThemeStyle.SecondaryText] = "\x1b[38;2;98;114;164m",  // Comment
            [ConsoleThemeStyle.TertiaryText] = "\x1b[38;2;98;114;164m",   // Comment
            [ConsoleThemeStyle.Invalid] = "\x1b[38;2;241;250;140m",       // Yellow
            [ConsoleThemeStyle.Null] = "\x1b[38;2;139;233;253m",          // Cyan
            [ConsoleThemeStyle.Name] = "\x1b[38;2;139;233;253m",          // Cyan
            [ConsoleThemeStyle.String] = "\x1b[38;2;241;250;140m",        // Yellow
            [ConsoleThemeStyle.Number] = "\x1b[38;2;189;147;249m",        // Purple
            [ConsoleThemeStyle.Boolean] = "\x1b[38;2;189;147;249m",       // Purple
            [ConsoleThemeStyle.Scalar] = "\x1b[38;2;189;147;249m",        // Purple
            [ConsoleThemeStyle.LevelVerbose] = "\x1b[37m",                // White
            [ConsoleThemeStyle.LevelDebug] = "\x1b[37m",                  // White
            [ConsoleThemeStyle.LevelInformation] = "\x1b[38;2;80;250;123m", // Green
            [ConsoleThemeStyle.LevelWarning] = "\x1b[38;2;255;184;108m",  // Orange
            [ConsoleThemeStyle.LevelError] = "\x1b[38;2;255;85;85m",      // Red
            [ConsoleThemeStyle.LevelFatal] = "\x1b[38;2;255;85;85m",      // Red
        });

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
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                theme: DraculaTheme));

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
