using Bcommerce.BuildingBlocks.Web.Filters;
using Bcommerce.BuildingBlocks.Web.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.BuildingBlocks.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksWeb(this IServiceCollection services)
    {
        // Registra Middlewares (se forem factory-based ou precisar de DI)
        services.AddScoped<ExceptionHandlingMiddleware>();
        services.AddScoped<RequestLoggingMiddleware>();
        services.AddScoped<CorrelationIdMiddleware>();
        services.AddScoped<PerformanceMonitoringMiddleware>();
        services.AddScoped<TenantResolutionMiddleware>();

        // Configura Controllers e Filtros Globais
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            // Suprime a validação automática para usarmos nosso ValidationFilter/Middleware
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }
}
