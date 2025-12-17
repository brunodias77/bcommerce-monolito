using Bcommerce.BuildingBlocks.Application.Models;

namespace Bcommerce.BuildingBlocks.Web.Models;

/// <summary>
/// Representação padronizada de erros na API.
/// </summary>
/// <remarks>
/// Contém detalhes sobre o erro ocorrido.
/// - Segue padrão similar ao ProblemDetails
/// - Inclui Código, Mensagem e Detalhes técnicos
/// 
/// Exemplo de uso:
/// <code>
/// new ErrorResponse("Auth.Error", "Acesso negado");
/// </code>
/// </remarks>
public record ErrorResponse(string Code, string Message, string? Details = null, ErrorType Type = ErrorType.Failure)
{
    public static ErrorResponse FromError(Error error) => new(error.Code, error.Description, null, error.Type);
}
