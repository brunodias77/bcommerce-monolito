using Bcommerce.BuildingBlocks.Application.Models;
using FluentValidation.Results;

namespace Bcommerce.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exceção para erros de validação de dados (HTTP 400).
/// </summary>
/// <remarks>
/// Agrupa múltiplos erros de validação (FluentValidation).
/// - Contém dicionário de campos inválidos e mensagens
/// - Retornada automaticamente pelo ValidationBehavior
/// - Mapeada para ProblemDetails com extensions de erro
/// 
/// Exemplo de uso:
/// <code>
/// // Geralmente lançada automaticamente, mas pode ser manual:
/// var falhas = result.Errors;
/// if (falhas.Any()) throw new ValidationException(falhas);
/// </code>
/// </remarks>
public class ValidationException : ApplicationException
{
    /// <summary>
    /// Cria uma nova instância de <see cref="ValidationException"/> a partir de falhas de validação.
    /// </summary>
    /// <param name="failures">Coleção de falhas de validação do FluentValidation.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Erro de Validação", "Um ou mais erros de validação ocorreram.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    /// <summary>
    /// Dicionário contendo os erros de validação agrupados por nome da propriedade.
    /// A chave é o nome da propriedade e o valor é um array de mensagens de erro.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
}
