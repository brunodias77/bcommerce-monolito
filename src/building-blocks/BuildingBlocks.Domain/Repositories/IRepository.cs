using BuildingBlocks.Domain.Entities;

namespace BuildingBlocks.Domain.Repositories;

/// <summary>
/// Interface marcadora para repositórios.
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade gerenciada pelo repositório</typeparam>
/// <remarks>
/// No padrão Repository + Unit of Work com Entity Framework Core:
/// - Repositórios encapsulam consultas específicas de domínio
/// - DbContext atua como Unit of Work implícito
/// - SaveChangesAsync é chamado via IUnitOfWork
/// 
/// Implementações específicas devem adicionar métodos de acordo com necessidades do domínio.
/// 
/// Exemplo de uso:
/// <code>
/// public interface IProductRepository : IRepository&lt;Product&gt;
/// {
///     Task&lt;Product?&gt; GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
///     Task&lt;Product?&gt; GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
///     Task&lt;IReadOnlyList&lt;Product&gt;&gt; GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
///     Task AddAsync(Product product, CancellationToken cancellationToken = default);
///     void Update(Product product);
///     void Remove(Product product);
/// }
/// </code>
/// </remarks>
public interface IRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Unit of Work associado ao repositório.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}
