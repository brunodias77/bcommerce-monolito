using Bcommerce.BuildingBlocks.Web.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Bcommerce.BuildingBlocks.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseBuildingBlocksWeb(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>(); // Importante vir cedo para pegar erros
        app.UseMiddleware<PerformanceMonitoringMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();

        return app;
    }
}
