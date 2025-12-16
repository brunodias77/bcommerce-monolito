using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Presentation.Middleware;

/// <summary>
/// Middleware para logging de requisições HTTP.
/// </summary>
/// <remarks>
/// Registra:
/// - Método HTTP, Path, Query String
/// - Tempo de processamento
/// - Status Code da resposta
/// - User Agent, Client IP
/// - Trace ID para correlação
/// 
/// Formato de log:
/// <code>
/// HTTP GET /api/users?page=1 responded 200 in 45ms [TraceId: abc123]
/// </code>
/// 
/// Registro:
/// <code>
/// app.UseRequestLoggingMiddleware();
/// </code>
/// 
/// Configuração de nível de log:
/// - Information: Requisições normais
/// - Warning: Respostas 4xx
/// - Error: Respostas 5xx e requisições lentas (&gt; 5s)
/// </remarks>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        RequestLoggingOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RequestLoggingOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging para health checks e outros paths ignorados
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;

        // Log início (opcional)
        if (_options.LogRequestStart)
        {
            _logger.LogDebug(
                "HTTP {Method} {Path} iniciado [TraceId: {TraceId}]",
                context.Request.Method,
                GetPath(context.Request),
                traceId);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            LogResponse(context, stopwatch.ElapsedMilliseconds, traceId);
        }
    }

    private void LogResponse(HttpContext context, long elapsedMs, string traceId)
    {
        var statusCode = context.Response.StatusCode;
        var path = GetPath(context.Request);
        var method = context.Request.Method;

        // Determina nível de log baseado no status code e tempo
        var logLevel = GetLogLevel(statusCode, elapsedMs);

        _logger.Log(logLevel,
            "HTTP {Method} {Path} respondeu {StatusCode} em {ElapsedMilliseconds}ms [TraceId: {TraceId}]",
            method,
            path,
            statusCode,
            elapsedMs,
            traceId);

        // Log adicional para requisições lentas
        if (elapsedMs > _options.SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "REQUISIÇÃO LENTA: {Method} {Path} levou {ElapsedMilliseconds}ms (limite: {Threshold}ms) [TraceId: {TraceId}]",
                method,
                path,
                elapsedMs,
                _options.SlowRequestThresholdMs,
                traceId);
        }
    }

    private static LogLevel GetLogLevel(int statusCode, long elapsedMs)
    {
        if (elapsedMs > 5000) // > 5 segundos é erro
            return LogLevel.Error;

        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }

    private bool ShouldSkip(PathString path)
    {
        foreach (var ignoredPath in _options.IgnoredPaths)
        {
            if (path.StartsWithSegments(ignoredPath))
                return true;
        }

        return false;
    }

    private static string GetPath(HttpRequest request)
    {
        return request.QueryString.HasValue
            ? $"{request.Path}{request.QueryString}"
            : request.Path.ToString();
    }
}

/// <summary>
/// Opções para configuração do RequestLoggingMiddleware.
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// Se true, loga quando a requisição inicia (Debug level).
    /// </summary>
    public bool LogRequestStart { get; set; } = false;

    /// <summary>
    /// Threshold em ms para considerar requisição lenta.
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 3000;

    /// <summary>
    /// Paths a serem ignorados no logging (ex: health checks).
    /// </summary>
    public List<string> IgnoredPaths { get; set; } = new()
    {
        "/health",
        "/healthz",
        "/ready",
        "/metrics",
        "/swagger",
        "/favicon.ico"
    };
}

/// <summary>
/// Extensão para registrar o middleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLoggingMiddleware(
        this IApplicationBuilder app,
        Action<RequestLoggingOptions>? configure = null)
    {
        var options = new RequestLoggingOptions();
        configure?.Invoke(options);

        return app.UseMiddleware<RequestLoggingMiddleware>(options);
    }
}
