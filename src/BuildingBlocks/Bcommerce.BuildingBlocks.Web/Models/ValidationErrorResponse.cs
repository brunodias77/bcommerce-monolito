namespace Bcommerce.BuildingBlocks.Web.Models;

/// <summary>
/// Resposta específica para erros de validação.
/// </summary>
/// <remarks>
/// Estende ErrorResponse para incluir detalhes por campo.
/// - Dicionário de erros (Campo -> Mensagens)
/// - Usado em respostas 400 Bad Request
/// 
/// Exemplo de uso:
/// <code>
/// new ValidationErrorResponse("Validation", "Erro", errorsDictionary);
/// </code>
/// </remarks>
public record ValidationErrorResponse(string Code, string Message, IDictionary<string, string[]> Errors) 
    : ErrorResponse(Code, Message);
