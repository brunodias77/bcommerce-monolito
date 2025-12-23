using Bcommerce.BuildingBlocks.Application.Exceptions;
using Bcommerce.BuildingBlocks.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Bcommerce.BuildingBlocks.Web.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções.
/// </summary>
/// <remarks>
/// Captura erros não tratados no pipeline e retorna respostas JSON padronizadas.
/// - Converte exceções de domínio em status HTTP adequados
/// - Garante que nenhuma stack trace vaze em produção (exceto via log)
/// 
/// Exemplo de uso:
/// <code>
/// app.UseMiddleware&lt;ExceptionHandlingMiddleware&gt;();
/// </code>
/// </remarks>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu uma exceção não tratada na requisição.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => 
                new ValidationErrorResponse("Validation.Error", "Erro de validação", validationEx.Errors),

            NotFoundException notFoundEx =>
                new ErrorResponse("Resource.NotFound", notFoundEx.Message, null, Application.Models.ErrorType.NotFound),
            
            UnauthorizedException unauthorizedEx =>
                new ErrorResponse("Auth.Unauthorized", unauthorizedEx.Message, null, Application.Models.ErrorType.Unauthorized),

            ForbiddenException forbiddenEx =>
                new ErrorResponse("Auth.Forbidden", forbiddenEx.Message, null, Application.Models.ErrorType.Forbidden),

            Application.Exceptions.ApplicationException appEx => 
                new ErrorResponse("Application.Error", appEx.Message),
            
            _ => new ErrorResponse("Server.Error", "Ocorreu um erro interno no servidor.", exception.Message)
        };

        context.Response.StatusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            NotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedException => (int)HttpStatusCode.Unauthorized,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            Application.Exceptions.ApplicationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
