using BuildingBlocks.Application.Models;
using BuildingBlocks.Application.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web.Controllers;

/// <summary>
/// Controller base para APIs REST
/// Fornece métodos helper para respostas padronizadas
///
/// Todos os controllers de API devem herdar desta classe para garantir
/// consistência nas respostas e tratamento de erros
///
/// Exemplos de uso baseados no schema SQL:
///
/// Query bem-sucedida:
/// - return Ok(produto);                    // 200 OK com o produto
/// - return Ok(listaProdutos);              // 200 OK com lista
/// - return OkPaginated(produtosPaginados); // 200 OK com paginação
///
/// Comando bem-sucedido:
/// - return Created(produto);               // 201 Created
/// - return NoContent();                    // 204 No Content (update/delete)
///
/// Erros:
/// - return NotFound("PRODUTO_NAO_ENCONTRADO", "Produto não encontrado");
/// - return BadRequest("PRECO_INVALIDO", "O preço deve ser maior que zero");
/// - return Conflict("ESTOQUE_INSUFICIENTE", "Estoque insuficiente");
///
/// Usando Result pattern:
/// var resultado = await mediator.Send(comando);
/// return resultado.IsSuccess
///     ? Ok(resultado.Value)
///     : HandleFailure(resultado);
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Retorna resposta 200 OK com dados
    /// </summary>
    /// <typeparam name="T">Tipo do dado de resposta</typeparam>
    /// <param name="data">Dados a serem retornados</param>
    protected IActionResult Ok<T>(T data) => base.Ok(data);

    /// <summary>
    /// Retorna resposta 200 OK com dados paginados
    /// Inclui metadados de paginação no cabeçalho X-Pagination
    /// </summary>
    /// <typeparam name="T">Tipo dos itens na lista</typeparam>
    /// <param name="pagedResult">Resultado paginado</param>
    protected IActionResult OkPaginated<T>(IPaginatedResult<T> pagedResult)
    {
        // Adiciona metadados de paginação no header
        var metadata = new
        {
            pagedResult.CurrentPage,
            pagedResult.PageSize,
            pagedResult.TotalPages,
            pagedResult.TotalCount,
            pagedResult.HasPrevious,
            pagedResult.HasNext
        };

        Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metadata));

        return base.Ok(pagedResult);
    }

    /// <summary>
    /// Retorna resposta 201 Created com o recurso criado
    /// Inclui header Location com a URI do novo recurso
    /// </summary>
    /// <typeparam name="T">Tipo do recurso criado</typeparam>
    /// <param name="resource">Recurso criado</param>
    /// <param name="routeName">Nome da rota para obter o recurso (opcional)</param>
    /// <param name="routeValues">Valores da rota (opcional)</param>
    protected IActionResult Created<T>(T resource, string? routeName = null, object? routeValues = null)
    {
        if (routeName is not null && routeValues is not null)
        {
            return CreatedAtRoute(routeName, routeValues, resource);
        }

        return StatusCode(StatusCodes.Status201Created, resource);
    }

    /// <summary>
    /// Retorna resposta 204 No Content
    /// Usado para indicar que a operação foi bem-sucedida mas não há conteúdo para retornar
    /// Comum em operações de UPDATE e DELETE
    /// </summary>
    protected new IActionResult NoContent() => base.NoContent();

    /// <summary>
    /// Retorna resposta 400 Bad Request com erro de validação
    /// </summary>
    /// <param name="code">Código do erro</param>
    /// <param name="message">Mensagem do erro</param>
    protected IActionResult BadRequest(string code, string message)
    {
        return base.BadRequest(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Erro de validação",
            Status = StatusCodes.Status400BadRequest,
            Detail = message,
            Extensions = { ["code"] = code }
        });
    }

    /// <summary>
    /// Retorna resposta 404 Not Found
    /// </summary>
    /// <param name="code">Código do erro</param>
    /// <param name="message">Mensagem do erro</param>
    protected IActionResult NotFound(string code, string message)
    {
        return base.NotFound(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Recurso não encontrado",
            Status = StatusCodes.Status404NotFound,
            Detail = message,
            Extensions = { ["code"] = code }
        });
    }

    /// <summary>
    /// Retorna resposta 409 Conflict
    /// Usado para violações de regras de negócio
    /// </summary>
    /// <param name="code">Código do erro</param>
    /// <param name="message">Mensagem do erro</param>
    protected IActionResult Conflict(string code, string message)
    {
        return base.Conflict(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Title = "Conflito",
            Status = StatusCodes.Status409Conflict,
            Detail = message,
            Extensions = { ["code"] = code }
        });
    }

    /// <summary>
    /// Retorna resposta 401 Unauthorized
    /// Usado quando o usuário não está autenticado
    /// </summary>
    /// <param name="code">Código do erro</param>
    /// <param name="message">Mensagem do erro</param>
    protected IActionResult Unauthorized(string code, string message)
    {
        return base.Unauthorized(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Não autenticado",
            Status = StatusCodes.Status401Unauthorized,
            Detail = message,
            Extensions = { ["code"] = code }
        });
    }

    /// <summary>
    /// Retorna resposta 403 Forbidden
    /// Usado quando o usuário está autenticado mas não tem permissão
    /// </summary>
    /// <param name="code">Código do erro</param>
    /// <param name="message">Mensagem do erro</param>
    protected IActionResult Forbidden(string code, string message)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Acesso negado",
            Status = StatusCodes.Status403Forbidden,
            Detail = message,
            Extensions = { ["code"] = code }
        });
    }

    /// <summary>
    /// Converte um Result em resposta HTTP apropriada
    /// Mapeia automaticamente o tipo de erro para o status code correto
    /// </summary>
    /// <param name="result">Resultado da operação</param>
    protected IActionResult HandleFailure(Result result)
    {
        return result.Error.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error.Code, result.Error.Message),
            ErrorType.NotFound => NotFound(result.Error.Code, result.Error.Message),
            ErrorType.Conflict => Conflict(result.Error.Code, result.Error.Message),
            ErrorType.Unauthorized => Unauthorized(result.Error.Code, result.Error.Message),
            ErrorType.Forbidden => Forbidden(result.Error.Code, result.Error.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Erro interno do servidor",
                Status = StatusCodes.Status500InternalServerError,
                Detail = result.Error.Message,
                Extensions = { ["code"] = result.Error.Code }
            })
        };
    }

    /// <summary>
    /// Converte um Result com valor em resposta HTTP apropriada
    /// Retorna 200 OK se sucesso, ou erro apropriado se falha
    /// </summary>
    /// <typeparam name="T">Tipo do valor de retorno</typeparam>
    /// <param name="result">Resultado da operação</param>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        return result.IsSuccess
            ? Ok(result.Value)
            : HandleFailure(result);
    }

    /// <summary>
    /// Converte um Result com valor em resposta HTTP 201 Created
    /// Usado para operações de criação
    /// </summary>
    /// <typeparam name="T">Tipo do recurso criado</typeparam>
    /// <param name="result">Resultado da operação</param>
    /// <param name="routeName">Nome da rota para obter o recurso (opcional)</param>
    /// <param name="routeValues">Valores da rota (opcional)</param>
    protected IActionResult HandleCreated<T>(
        Result<T> result,
        string? routeName = null,
        object? routeValues = null)
    {
        return result.IsSuccess
            ? Created(result.Value, routeName, routeValues)
            : HandleFailure(result);
    }
}
