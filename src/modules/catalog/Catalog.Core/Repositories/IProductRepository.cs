using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;
using Catalog.Core.Enums;

namespace Catalog.Core.Repositories;

/// <summary>
/// Interface do repositório de produtos.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Busca um produto por SKU.
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um produto por slug.
    /// </summary>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um produto com todas as imagens.
    /// </summary>
    Task<Product?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um produto com todos os relacionamentos.
    /// </summary>
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos por categoria.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos por marca.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos por status.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos em destaque.
    /// </summary>
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int take = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos com estoque baixo.
    /// </summary>
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um SKU já existe.
    /// </summary>
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca produtos com reservas expiradas.
    /// </summary>
    Task<IReadOnlyList<Product>> GetWithExpiredReservationsAsync(CancellationToken cancellationToken = default);
}
