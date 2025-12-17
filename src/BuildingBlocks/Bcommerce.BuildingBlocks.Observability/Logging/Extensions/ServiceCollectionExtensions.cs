using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.BuildingBlocks.Observability.Logging.Extensions;

/// <summary>
/// Extensões para registro de serviços de logging no DI.
/// </summary>
/// <remarks>
/// Registra dependências necessárias para os Enrichers do Serilog.
/// - Adiciona IHttpContextAccessor
/// - Registra Enrichers customizados (CorrelationId, UserContext)
/// 
/// Exemplo de uso:
/// <code>
/// services.AddLoggingServices();
/// </code>
/// </remarks>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        // Register enrichers if needed via DI, but Serilog usually instantiates them or uses context accessor directly.
        // For simple enrichers they can be transient.
        services.AddTransient<SerilogEnrichers.CorrelationIdEnricher>();
        services.AddTransient<SerilogEnrichers.UserContextEnricher>();
        
        return services;
    }
}
