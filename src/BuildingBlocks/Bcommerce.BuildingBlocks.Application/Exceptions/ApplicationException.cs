namespace Bcommerce.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exceção base abstrata para erros conhecidos da aplicação.
/// </summary>
/// <remarks>
/// Ponto central para exceções que devem ser tratadas de forma customizada pelo middleware.
/// - Define Title e Message para respostas HTTP padronizadas
/// - Herdada por NotFound, Validation, Conflict, etc.
/// - Não deve ser lançada diretamente (use as especializações)
/// 
/// Exemplo de uso:
/// <code>
/// // Middleware de tratamento:
/// catch (ApplicationException ex)
/// {
///     return Problem(title: ex.Title, detail: ex.Message);
/// }
/// </code>
/// </remarks>
/// <param name="title">Título do erro.</param>
/// <param name="message">Detalhe do erro.</param>
public abstract class ApplicationException(string title, string message) : Exception(message)
{
    /// <summary>
    /// Título categorizado do erro, usado para classificação e exibição ao usuário.
    /// </summary>
    public string Title { get; } = title;
}
