using BuildingBlocks.Presentation.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Presentation.Filters;

/// <summary>
/// Filtro global para tratamento de exceções não capturadas.
/// </summary>
/// <remarks>
/// Este filtro:
/// 1. Captura exceções lançadas em controllers/actions
/// 2. Loga a exceção com detalhes contextuais
/// 3. Retorna ProblemDetails padronizado (RFC 7807)
/// 4. Oculta stack trace em produção
/// 
/// Registro:
/// <code>
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;ExceptionHandlingFilter&gt;();
/// });
/// </code>
/// 
/// Ou via atributo:
/// <code>
/// [ServiceFilter(typeof(ExceptionHandlingFilter))]
/// public class MyController : ApiControllerBase { }
/// </code>
/// </remarks>
public class ExceptionHandlingFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionHandlingFilter> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingFilter(
        ILogger<ExceptionHandlingFilter> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var httpContext = context.HttpContext;

        // Log detalhado
        _logger.LogError(exception,
            "Unhandled exception. Path: {Path}, Method: {Method}, TraceId: {TraceId}",
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.TraceIdentifier);

        // Determina status code baseado no tipo de exceção
        var (statusCode, errorCode) = GetExceptionDetails(exception);

        // Cria ProblemDetails
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request",
            Status = statusCode,
            Detail = _environment.IsDevelopment()
                ? exception.Message
                : "An internal error occurred. Please try again later.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // Inclui stack trace apenas em desenvolvimento
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.ToString();
        }

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }

    private static (int StatusCode, string ErrorCode) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "ARGUMENT_NULL"),
            ArgumentException => (StatusCodes.Status400BadRequest, "ARGUMENT_INVALID"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "UNAUTHORIZED"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "INVALID_OPERATION"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND"),
            NotSupportedException => (StatusCodes.Status405MethodNotAllowed, "NOT_SUPPORTED"),
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "TIMEOUT"),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "CANCELLED"),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR")
        };
    }
}

/// <summary>
/// Custom status code 499 - Client Closed Request (usado pelo nginx).
/// </summary>
public static class StatusCodes499
{
    public const int Status499ClientClosedRequest = 499;
}
