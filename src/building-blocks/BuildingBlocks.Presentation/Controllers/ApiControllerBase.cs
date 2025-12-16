using BuildingBlocks.Application.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Presentation.Controllers;

/// <summary>
/// Base controller que padroniza respostas HTTP para todos os controllers da API.
/// </summary>
/// <remarks>
/// Funcionalidades:
/// - Herança do ControllerBase com atributos padrão
/// - Métodos helper para converter Result em IActionResult
/// - Injeção de IMediator para CQRS
/// 
/// Uso:
/// <code>
/// [Route("api/users")]
/// public class UsersController : ApiControllerBase
/// {
///     [HttpGet("{id}")]
///     public async Task&lt;IActionResult&gt; GetById(Guid id)
///     {
///         var result = await Mediator.Send(new GetUserByIdQuery(id));
///         return HandleResult(result);
///     }
/// }
/// </code>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Acesso ao MediatR para enviar Commands e Queries.
    /// </summary>
    protected IMediator Mediator => _mediator ??=
        HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Converte Result sem valor em IActionResult.
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return HandleError(result.Error);
    }

    /// <summary>
    /// Converte Result com valor em IActionResult.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return HandleError(result.Error);
    }

    /// <summary>
    /// Converte Result com valor em IActionResult com status 201 Created.
    /// </summary>
    protected IActionResult HandleCreatedResult<T>(
        Result<T> result,
        string actionName,
        object? routeValues = null)
    {
        if (result.IsSuccess)
            return CreatedAtAction(actionName, routeValues, result.Value);

        return HandleError(result.Error);
    }

    /// <summary>
    /// Converte Result com valor em IActionResult com status 201 Created usando URI.
    /// </summary>
    protected IActionResult HandleCreatedResult<T>(
        Result<T> result,
        Uri location)
    {
        if (result.IsSuccess)
            return Created(location, result.Value);

        return HandleError(result.Error);
    }

    /// <summary>
    /// Converte Result com valor em IActionResult com status 202 Accepted.
    /// </summary>
    protected IActionResult HandleAcceptedResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Accepted(result.Value);

        return HandleError(result.Error);
    }

    /// <summary>
    /// Converte Error em IActionResult com ProblemDetails.
    /// </summary>
    protected IActionResult HandleError(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Type = GetErrorTypeUri(error.Type),
            Title = GetErrorTitle(error.Type),
            Status = statusCode,
            Detail = error.Message,
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = error.Code;
        problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private static string GetErrorTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Erro de Validação",
        ErrorType.NotFound => "Recurso Não Encontrado",
        ErrorType.Conflict => "Conflito",
        ErrorType.Unauthorized => "Não Autorizado",
        ErrorType.Forbidden => "Proibido",
        ErrorType.Failure => "Erro Interno do Servidor",
        _ => "Ocorreu um erro"
    };

    private static string GetErrorTypeUri(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
