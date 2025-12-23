namespace Bcommerce.BuildingBlocks.Application.Abstractions.Data;

/// <summary>
/// Contrato para o padrão Unit of Work.
/// </summary>
/// <remarks>
/// Garante que todas as alterações sejam persistidas em uma única transação.
/// - Mantém consistência dos dados (ACID)
/// - Deve ser chamado apenas uma vez por requisição/comando
/// - Geralmente implementado pelo DbContext do EF Core
/// 
/// Exemplo de uso:
/// <code>
/// // No final do Handler:
/// await _repository.AddAsync(entity);
/// await _unitOfWork.SaveChangesAsync(cancellationToken);
/// </code>
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todas as alterações pendentes no banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Número de registros afetados.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
