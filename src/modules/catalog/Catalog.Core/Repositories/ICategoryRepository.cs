using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;

namespace Catalog.Core.Repositories;

/// <summary>
/// Interface do repositório de categorias.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Busca uma categoria por slug.
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca categorias raiz (sem parent).
    /// </summary>
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca subcategorias de uma categoria.
    /// </summary>
    Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca todas as categorias ativas.
    /// </summary>
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca a árvore completa de categorias.
    /// </summary>
    Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca categoria com subcategorias.
    /// </summary>
    Task<Category?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um slug já existe.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
