using BuildingBlocks.Domain.Repositories;
using Cart.Core.Entities;

namespace Cart.Core.Repositories;

/// <summary>
/// Interface do repositório de carrinhos.
/// </summary>
public interface ICartRepository : IRepository<Entities.Cart>
{
    /// <summary>
    /// Busca um carrinho por ID.
    /// </summary>
    Task<Entities.Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um carrinho ativo por ID de usuário.
    /// </summary>
    Task<Entities.Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um carrinho ativo por ID de sessão.
    /// </summary>
    Task<Entities.Cart?> GetActiveBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um carrinho com itens incluídos.
    /// </summary>
    Task<Entities.Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário tem um carrinho ativo.
    /// </summary>
    Task<bool> UserHasActiveCartAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo carrinho.
    /// </summary>
    Task AddAsync(Entities.Cart cart, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um carrinho existente.
    /// </summary>
    void Update(Entities.Cart cart);
}
