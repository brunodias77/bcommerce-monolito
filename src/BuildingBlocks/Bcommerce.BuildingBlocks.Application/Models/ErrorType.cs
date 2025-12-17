namespace Bcommerce.BuildingBlocks.Application.Models;

/// <summary>
/// Categoriza os tipos de erro para mapeamento correto de respostas HTTP.
/// </summary>
/// <remarks>
/// Usado pelo middleware para converter Result.Failure em Status Code.
/// - Failure = 500
/// - Validation = 400
/// - NotFound = 404
/// - Conflict = 409
/// - Unauthorized = 401
/// - Forbidden = 403
/// 
/// Exemplo de uso:
/// <code>
/// var erro = new Error("Code", "Msg", ErrorType.NotFound);
/// </code>
/// </remarks>
public enum ErrorType
{
    /// <summary>Erro genérico de sistema (bug, falha inesperada).</summary>
    Failure = 0,
    /// <summary>Erro de validação de dados de entrada.</summary>
    Validation = 1,
    /// <summary>Recurso/entidade não encontrado.</summary>
    NotFound = 2,
    /// <summary>Conflito de estado (duplicidade, concorrência).</summary>
    Conflict = 3,
    /// <summary>Usuário não autenticado.</summary>
    Unauthorized = 4,
    /// <summary>Usuário autenticado mas sem permissão.</summary>
    Forbidden = 5
}
