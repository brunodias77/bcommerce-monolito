using Bcommerce.BuildingBlocks.Application.Models;
using Bcommerce.BuildingBlocks.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.BuildingBlocks.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador base para APIs REST da aplicação.
/// </summary>
/// <remarks>
/// Fornece funcionalidades comuns para todos os controllers.
/// - Tratamento padronizado de resultados (Result Pattern)
/// - Métodos auxiliares para respostas HTTP
/// 
/// Exemplo de uso:
/// <code>
/// public class MyController(ISender sender) : ApiControllerBase(sender)
/// </code>
/// </remarks>
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    protected readonly ISender _sender = sender;

    protected IActionResult HandleFailure(Result result) =>
        result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException("Não é possível tratar sucesso como falha"),
            // Case for IValidationResult removed as it is not implemented in core. 
            // Validation errors are typically thrown as Exceptions (ValidationException) and handled by middleware.
            _ => result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(ErrorResponse.FromError(result.Error)),
                ErrorType.Validation => BadRequest(ErrorResponse.FromError(result.Error)),
                ErrorType.Conflict => Conflict(ErrorResponse.FromError(result.Error)),
                ErrorType.Unauthorized => Unauthorized(ErrorResponse.FromError(result.Error)),
                ErrorType.Forbidden => Forbid(),
                _ => BadRequest(ErrorResponse.FromError(result.Error))
            }
        };

    protected IActionResult OkResponse<T>(T data) => Ok(ApiResponse<T>.Ok(data));
}
