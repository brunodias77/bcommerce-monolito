using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Presentation.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções.
/// </summary>
/// <remarks>
/// Diferença entre Middleware e Filter:
/// - Middleware: Captura TODAS as exceções (incluindo de outros middlewares)
/// - Filter: Captura apenas exceções de Controllers/Actions
/// 
/// Use AMBOS para cobertura completa.
/// 
/// Registro:
/// <code>
/// app.UseExceptionHandler(); // built-in
/// // ou
/// app.UseMiddleware&lt;ExceptionHandlingMiddleware&gt;();
/// </code>
/// 
/// Ordem recomendada no pipeline:
/// <code>
/// app.UseExceptionHandlingMiddleware(); // 1. Mais externo
/// app.UseRequestLoggingMiddleware();    // 2. Logging
/// app.UseAuthentication();              // 3. Auth
/// app.UseAuthorization();               // 4. Authz
/// app.MapControllers();                 // 5. Controllers
/// </code>
/// </remarks>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log completo da exceção
        _logger.LogError(exception,
            "Unhandled exception occurred. " +
            "TraceId: {TraceId}, Path: {Path}, Method: {Method}, User: {User}",
            context.TraceIdentifier,
            context.Request.Path,
            context.Request.Method,
            context.User?.Identity?.Name ?? "Anonymous");

        // Determina status e código de erro
        var (statusCode, errorCode, message) = MapException(exception);

        // Cria ProblemDetails
        var problemDetails = new ProblemDetails
        {
            Type = GetTypeUri(statusCode),
            Title = GetTitle(statusCode),
            Status = statusCode,
            Detail = _environment.IsDevelopment() ? exception.Message : message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = new
            {
                message = exception.Message,
                type = exception.GetType().Name,
                stackTrace = exception.StackTrace?.Split('\n').Take(10).ToArray()
            };
        }

        // Escreve resposta
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private static (int StatusCode, string ErrorCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (
                StatusCodes.Status400BadRequest,
                "ARGUMENT_NULL",
                "A required argument was not provided."),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "ARGUMENT_INVALID",
                "An argument has an invalid value."),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "Authentication is required."),

            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "INVALID_OPERATION",
                "The operation is not valid for the current state."),

            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                "The requested resource was not found."),

            NotSupportedException => (
                StatusCodes.Status405MethodNotAllowed,
                "NOT_SUPPORTED",
                "The operation is not supported."),

            TimeoutException => (
                StatusCodes.Status504GatewayTimeout,
                "TIMEOUT",
                "The operation timed out."),

            OperationCanceledException => (
                499, // Client Closed Request
                "CANCELLED",
                "The request was cancelled."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                "An internal error occurred. Please try again later.")
        };
    }

    private static string GetTypeUri(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        405 => "https://tools.ietf.org/html/rfc7231#section-6.5.5",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        499 => "https://httpstatuses.com/499",
        504 => "https://tools.ietf.org/html/rfc7231#section-6.6.5",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        405 => "Method Not Allowed",
        409 => "Conflict",
        499 => "Client Closed Request",
        504 => "Gateway Timeout",
        _ => "Internal Server Error"
    };
}

/// <summary>
/// Extensão para registrar o middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
