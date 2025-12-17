using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Web.Middleware;

/// <summary>
/// Middleware para identificação de Tenant (Multi-tenancy).
/// </summary>
/// <remarks>
/// Extrai o ID do tenant a partir de headers da requisição.
/// - Lê "X-Tenant-Id"
/// - Prepara o contexto para isolamento de dados por cliente
/// 
/// Exemplo de uso:
/// <code>
/// app.UseMiddleware&lt;TenantResolutionMiddleware&gt;();
/// </code>
/// </remarks>
public class TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<TenantResolutionMiddleware> _logger = logger;
    private const string TenantHeader = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantHeader, out var tenantId))
        {
            _logger.LogInformation("Tenant identificado: {TenantId}", tenantId.ToString());
            // Aqui poderia setar o tenant num serviço de CurrentTenant (Scoped)
        }
        else
        {
            // Opcional: Rejeitar ou assumir default
            // _logger.LogWarning("Tenant não identificado na requisição");
        }

        await _next(context);
    }
}
