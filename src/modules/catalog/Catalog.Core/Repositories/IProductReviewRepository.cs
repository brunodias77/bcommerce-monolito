using BuildingBlocks.Domain.Repositories;
using Catalog.Core.Entities;

namespace Catalog.Core.Repositories;

/// <summary>
/// Interface do repositório de avaliações de produtos.
/// </summary>
public interface IProductReviewRepository : IRepository<ProductReview>
{
    /// <summary>
    /// Busca avaliações de um produto.
    /// </summary>
    Task<IReadOnlyList<ProductReview>> GetByProductIdAsync(
        Guid productId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca avaliações aprovadas de um produto.
    /// </summary>
    Task<IReadOnlyList<ProductReview>> GetApprovedByProductIdAsync(
        Guid productId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca avaliações de um usuário.
    /// </summary>
    Task<IReadOnlyList<ProductReview>> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca avaliações pendentes de aprovação.
    /// </summary>
    Task<IReadOnlyList<ProductReview>> GetPendingApprovalAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário já avaliou um produto.
    /// </summary>
    Task<bool> UserHasReviewedAsync(
        Guid userId, 
        Guid productId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula a média de avaliações de um produto.
    /// </summary>
    Task<double> GetAverageRatingAsync(
        Guid productId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta o total de avaliações aprovadas de um produto.
    /// </summary>
    Task<int> GetApprovedCountAsync(
        Guid productId, 
        CancellationToken cancellationToken = default);
}
