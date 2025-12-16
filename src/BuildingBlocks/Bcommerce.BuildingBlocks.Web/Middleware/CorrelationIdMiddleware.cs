using Microsoft.AspNetCore.Http;

namespace Bcommerce.BuildingBlocks.Web.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);

        // Adiciona ao contexto de rastreamento se necessário (ex: Serilog PushProperty)
        // Aqui apenas garantimos que o header exista na resposta

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string GetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
