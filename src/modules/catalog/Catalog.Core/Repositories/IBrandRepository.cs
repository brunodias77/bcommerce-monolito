using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;

namespace Catalog.Core.Repositories;

/// <summary>
/// Interface do repositório de marcas.
/// </summary>
public interface IBrandRepository : IRepository<Brand>
{
    /// <summary>
    /// Busca uma marca por slug.
    /// </summary>
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca uma marca por nome.
    /// </summary>
    Task<Brand?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca todas as marcas ativas.
    /// </summary>
    Task<IReadOnlyList<Brand>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um slug já existe.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
