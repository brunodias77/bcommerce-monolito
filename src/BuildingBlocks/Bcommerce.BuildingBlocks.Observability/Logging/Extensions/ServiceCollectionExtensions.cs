using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.BuildingBlocks.Observability.Logging.Extensions;

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
