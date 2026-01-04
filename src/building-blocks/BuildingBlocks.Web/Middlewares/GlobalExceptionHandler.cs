using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.Middlewares;

/// <summary>
/// Middleware global para tratamento de exceções não capturadas
/// Implementa RFC 7807 (Problem Details for HTTP APIs)
///
/// Este middleware captura todas as exceções não tratadas e as converte
/// em respostas HTTP padronizadas no formato ProblemDetails
///
/// Exemplos de exceções que podem ocorrer:
/// - DbUpdateException: Erro ao salvar no banco de dados
/// - UnauthorizedAccessException: Acesso não autorizado
/// - InvalidOperationException: Operação inválida (ex: pedido já pago)
/// - ArgumentException: Argumentos inválidos
/// - TimeoutException: Timeout em operação externa (gateway de pagamento)
/// - HttpRequestException: Erro em chamada HTTP externa
///
/// Cenários do e-commerce que podem gerar exceções:
/// - Timeout ao processar pagamento com gateway externo
/// - Erro de conexão com banco de dados durante checkout
/// - Violação de constraint no banco (ex: FK, unique)
/// - Deadlock em atualização de estoque concorrente
/// - Serviço de cálculo de frete indisponível
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Trata a exceção e retorna uma resposta padronizada
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Loga a exceção com detalhes completos
        _logger.LogError(
            exception,
            "Exceção não tratada: {ExceptionType} - {Message} - TraceId: {TraceId}",
            exception.GetType().Name,
            exception.Message,
            httpContext.TraceIdentifier);

        // Cria ProblemDetails baseado no tipo de exceção
        var problemDetails = CreateProblemDetails(httpContext, exception);

        // Define status code da resposta
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        // Retorna resposta em JSON
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // true indica que a exceção foi tratada
        return true;
    }

    /// <summary>
    /// Cria ProblemDetails apropriado baseado no tipo de exceção
    /// </summary>
    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var statusCode = DetermineStatusCode(exception);

        var problemDetails = new ProblemDetails
        {
            Type = DetermineType(statusCode),
            Title = DetermineTitle(exception, statusCode),
            Status = statusCode,
            Detail = DetermineDetail(exception),
            Instance = context.Request.Path
        };

        // Adiciona TraceId para rastreabilidade
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // Adiciona código de erro customizado se disponível
        if (exception.Data.Contains("ErrorCode"))
        {
            problemDetails.Extensions["code"] = exception.Data["ErrorCode"];
        }

        // Em desenvolvimento, adiciona stack trace e detalhes adicionais
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;

            // Adiciona inner exceptions
            if (exception.InnerException is not null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    Type = exception.InnerException.GetType().FullName,
                    Message = exception.InnerException.Message,
                    StackTrace = exception.InnerException.StackTrace
                };
            }
        }

        return problemDetails;
    }

    /// <summary>
    /// Determina o status code HTTP apropriado baseado no tipo de exceção
    /// </summary>
    private static int DetermineStatusCode(Exception exception)
    {
        // Verifica primeiro os tipos específicos
        if (exception is UnauthorizedAccessException) return StatusCodes.Status401Unauthorized;
        if (exception is ArgumentNullException) return StatusCodes.Status400BadRequest;
        if (exception is ArgumentException) return StatusCodes.Status400BadRequest;
        if (exception is InvalidOperationException) return StatusCodes.Status409Conflict;
        if (exception is TimeoutException) return StatusCodes.Status504GatewayTimeout;
        if (exception is NotImplementedException) return StatusCodes.Status501NotImplemented;
        if (exception is KeyNotFoundException) return StatusCodes.Status404NotFound;
        
        // Padrão para outros erros
        return StatusCodes.Status500InternalServerError;
    }

    /// <summary>
    /// Determina o título do erro baseado no status code
    /// </summary>
    private static string DetermineTitle(Exception exception, int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Requisição inválida",
            StatusCodes.Status401Unauthorized => "Não autenticado",
            StatusCodes.Status403Forbidden => "Acesso negado",
            StatusCodes.Status404NotFound => "Recurso não encontrado",
            StatusCodes.Status409Conflict => "Conflito",
            StatusCodes.Status500InternalServerError => "Erro interno do servidor",
            StatusCodes.Status501NotImplemented => "Não implementado",
            StatusCodes.Status504GatewayTimeout => "Timeout do gateway",
            _ => "Erro no servidor"
        };
    }

    /// <summary>
    /// Determina o tipo (URI) do problema baseado no status code
    /// Segue a especificação RFC 7231
    /// </summary>
    private static string DetermineType(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            StatusCodes.Status501NotImplemented => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            StatusCodes.Status504GatewayTimeout => "https://tools.ietf.org/html/rfc7231#section-6.6.5",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }

    /// <summary>
    /// Determina a mensagem de detalhe baseada na exceção
    /// Remove detalhes sensíveis em produção
    /// </summary>
    private string DetermineDetail(Exception exception)
    {
        // Em produção, retorna mensagens genéricas para não expor detalhes internos
        if (!_environment.IsDevelopment())
        {
            return exception switch
            {
                UnauthorizedAccessException => "Você não tem permissão para acessar este recurso",
                ArgumentException => "Os dados fornecidos são inválidos",
                InvalidOperationException => "A operação não pode ser realizada no estado atual do recurso",
                TimeoutException => "A operação demorou mais tempo que o esperado. Tente novamente",
                _ => "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde"
            };
        }

        // Em desenvolvimento, retorna a mensagem original da exceção
        return exception.Message;
    }
}
