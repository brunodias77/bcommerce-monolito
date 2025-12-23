using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Bcommerce.BuildingBlocks.Web.Middleware;

/// <summary>
/// Middleware para logging de requisições HTTP.
/// </summary>
/// <remarks>
/// Registra o início e fim de cada chamada à API.
/// - Inclui método, path, status code e duração da chamada
/// - Útil para diagnóstico de tráfego e debug
/// 
/// Exemplo de uso:
/// <code>
/// app.UseMiddleware&lt;RequestLoggingMiddleware&gt;();
/// </code>
/// </remarks>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Recebendo requisição: {Method} {Path}", context.Request.Method, context.Request.Path);

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation("Finalizando requisição: {Method} {Path} - Status: {StatusCode} - Duração: {ElapsedMilliseconds}ms", 
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
