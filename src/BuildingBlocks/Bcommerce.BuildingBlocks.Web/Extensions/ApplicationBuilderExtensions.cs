using Bcommerce.BuildingBlocks.Web.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Bcommerce.BuildingBlocks.Web.Extensions;

/// <summary>
/// Extensões para configuração do pipeline de requisição (Middleware).
/// </summary>
/// <remarks>
/// Adiciona middlewares essenciais da camada Web.
/// - Logging, CorrelationId, Exception Handling
/// - Monitoramento de performance e resolução de Tenant
/// 
/// Exemplo de uso:
/// <code>
/// app.UseBuildingBlocksWeb();
/// </code>
/// </remarks>
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
