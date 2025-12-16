using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Bcommerce.BuildingBlocks.Web.Middleware;

public class PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger = logger;
    private const int ThresholdMilliseconds = 500; // Alerta se passar de 500ms

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > ThresholdMilliseconds)
        {
            _logger.LogWarning("Requisição lenta detectada: {Method} {Path} levou {ElapsedMilliseconds}ms",
                context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
        }
    }
}
