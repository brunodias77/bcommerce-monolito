using BuildingBlocks.Application.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Presentation.Extensions;

/// <summary>
/// Extensões para construir ProblemDetails padronizados.
/// </summary>
/// <remarks>
/// Segue RFC 7807 - Problem Details for HTTP APIs.
/// 
/// Exemplo de resposta:
/// <code>
/// {
///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
///   "title": "Resource Not Found",
///   "status": 404,
///   "detail": "User with ID 123 was not found",
///   "instance": "/api/users/123",
///   "errorCode": "USER_NOT_FOUND",
///   "traceId": "00-abc123..."
/// }
/// </code>
/// </remarks>
public static class ProblemDetailsExtensions
{
    private static readonly Dictionary<ErrorType, string> ErrorTypeUris = new()
    {
        [ErrorType.Validation] = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        [ErrorType.NotFound] = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        [ErrorType.Conflict] = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        [ErrorType.Unauthorized] = "https://tools.ietf.org/html/rfc7235#section-3.1",
        [ErrorType.Forbidden] = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        [ErrorType.Failure] = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    /// <summary>
    /// Cria ProblemDetails a partir de um Error.
    /// </summary>
    public static ProblemDetails ToProblemDetails(
        this Error error,
        HttpContext? httpContext = null)
    {
        var statusCode = GetStatusCode(error.Type);

        var problemDetails = new ProblemDetails
        {
            Type = GetTypeUri(error.Type),
            Title = GetTitle(error.Type),
            Status = statusCode,
            Detail = error.Message,
            Instance = httpContext?.Request.Path
        };

        problemDetails.Extensions["errorCode"] = error.Code;

        if (httpContext != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        return problemDetails;
    }

    /// <summary>
    /// Cria ValidationProblemDetails para erros de validação.
    /// </summary>
    public static ValidationProblemDetails ToValidationProblemDetails(
        this Error error,
        HttpContext? httpContext = null)
    {
        var problemDetails = new ValidationProblemDetails
        {
            Type = ErrorTypeUris[ErrorType.Validation],
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = error.Message,
            Instance = httpContext?.Request.Path
        };

        // Parse validation errors from message (format: "PropertyName: Message; ...")
        var validationErrors = ParseValidationErrors(error.Message);
        foreach (var (property, messages) in validationErrors)
        {
            problemDetails.Errors[property] = messages.ToArray();
        }

        problemDetails.Extensions["errorCode"] = error.Code;

        if (httpContext != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        return problemDetails;
    }

    /// <summary>
    /// Cria ProblemDetails a partir de uma exceção.
    /// </summary>
    public static ProblemDetails ToProblemDetails(
        this Exception exception,
        HttpContext? httpContext = null,
        bool includeStackTrace = false)
    {
        var problemDetails = new ProblemDetails
        {
            Type = ErrorTypeUris[ErrorType.Failure],
            Title = "An unexpected error occurred",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message,
            Instance = httpContext?.Request.Path
        };

        problemDetails.Extensions["errorCode"] = "INTERNAL_ERROR";

        if (httpContext != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        if (includeStackTrace)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        return problemDetails;
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTypeUri(ErrorType errorType) =>
        ErrorTypeUris.TryGetValue(errorType, out var uri)
            ? uri
            : ErrorTypeUris[ErrorType.Failure];

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Resource Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Failure => "Internal Server Error",
        _ => "An error occurred"
    };

    private static Dictionary<string, List<string>> ParseValidationErrors(string message)
    {
        var result = new Dictionary<string, List<string>>();

        // Format: "PropertyName: Message; PropertyName2: Message2"
        var parts = message.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex > 0)
            {
                var property = part[..colonIndex].Trim();
                var errorMessage = part[(colonIndex + 1)..].Trim();

                if (!result.ContainsKey(property))
                    result[property] = new List<string>();

                result[property].Add(errorMessage);
            }
            else
            {
                // No property specified, use general key
                if (!result.ContainsKey(""))
                    result[""] = new List<string>();

                result[""].Add(part.Trim());
            }
        }

        return result;
    }
}
