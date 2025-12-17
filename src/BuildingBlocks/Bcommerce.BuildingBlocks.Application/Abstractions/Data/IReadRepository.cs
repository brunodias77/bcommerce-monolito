using Bcommerce.BuildingBlocks.Application.Models;
using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Bcommerce.BuildingBlocks.Domain.Specifications;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Data;

/// <summary>
/// Contrato para operações de leitura de dados (Query Side).
/// </summary>
/// <remarks>
/// Abstração focada apenas em leitura, segregando responsabilidades (ISP).
/// - Permite buscas por ID, listagem completa e filtragem por especificação
/// - Não permite alterações de estado (somente leitura)
/// - Deve ser implementada em infraestrutura usando EF Core ou Dapper
/// 
/// Exemplo de uso:
/// <code>
/// // Injeção de dependência:
/// constructor(IReadRepository&lt;Produto&gt; produtoReadRepo) { ... }
/// 
/// // Uso em QueryHandler:
/// var produto = await _produtoReadRepo.GetByIdAsync(query.Id);
/// </code>
/// </remarks>
public interface IReadRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Obtém uma entidade pelo seu identificador único.
    /// </summary>
    /// <param name="id">O identificador único da entidade.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A entidade se encontrada, ou null caso contrário.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todas as entidades da base de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Uma lista somente leitura contendo todas as entidades.</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém entidades que satisfazem a especificação fornecida.
    /// </summary>
    /// <param name="specification">Critérios de filtro encapsulados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de entidades filtradas.</returns>
    Task<IReadOnlyList<TEntity>> GetListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    
    // Suporte a paginação com retorno de PaginatedList (será implementado depois)
    // Task<PaginatedList<TEntity>> GetPagedListAsync(ISpecification<TEntity> specification, PagedRequest request, CancellationToken cancellationToken = default);
}
