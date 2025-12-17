namespace Bcommerce.BuildingBlocks.Application.Models;

/// <summary>
/// Representa um erro de domínio ou aplicação de forma estruturada e imutável.
/// </summary>
/// <remarks>
/// Usado no padrão Result para comunicar falhas sem lançar exceções.
/// - Erros são imutáveis (record) para thread-safety
/// - Use factory methods (Failure, Validation, etc) para criação
/// - Código deve seguir padrão "Entidade.Erro"
/// 
/// Exemplo de uso:
/// <code>
/// if (emailDuplicado)
///     return Result.Failure(Error.Conflict("User.EmailExists", "Email em uso"));
/// </code>
/// </remarks>
/// <param name="Code">Código único do erro no padrão "Entidade.TipoErro".</param>
/// <param name="Description">Descrição legível do erro para o usuário.</param>
/// <param name="Type">Categoria do erro para mapeamento de status HTTP.</param>
public record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    /// <summary>Representa ausência de erro (estado de sucesso).</summary>
    public static readonly Error None = new(string.Empty, string.Empty);
    /// <summary>Erro padrão para valores nulos inesperados.</summary>
    public static readonly Error NullValue = new("Error.NullValue", "O valor fornecido é nulo.", ErrorType.Failure);

    /// <summary>Cria um erro genérico de falha (HTTP 500).</summary>
    public static Error Failure(string code, string description) => 
        new(code, description, ErrorType.Failure);

    /// <summary>Cria um erro de validação de dados (HTTP 400).</summary>
    public static Error Validation(string code, string description) => 
        new(code, description, ErrorType.Validation);

    /// <summary>Cria um erro de recurso não encontrado (HTTP 404).</summary>
    public static Error NotFound(string code, string description) => 
        new(code, description, ErrorType.NotFound);

    /// <summary>Cria um erro de conflito de estado (HTTP 409).</summary>
    public static Error Conflict(string code, string description) => 
        new(code, description, ErrorType.Conflict);
        
    /// <summary>Cria um erro de autenticação (HTTP 401).</summary>
    public static Error Unauthorized(string code, string description) => 
        new(code, description, ErrorType.Unauthorized);

    /// <summary>Cria um erro de autorização/permissão (HTTP 403).</summary>
    public static Error Forbidden(string code, string description) => 
        new(code, description, ErrorType.Forbidden);
}
