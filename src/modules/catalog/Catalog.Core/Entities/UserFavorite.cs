using BuildingBlocks.Domain.Entities;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Produto Favorito.
/// Corresponde à tabela catalog.user_favorites no banco de dados.
/// </summary>
public class UserFavorite : Entity
{
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    /// <summary>
    /// Snapshot do produto no momento da adição aos favoritos.
    /// Armazena nome, preço e imagem para exibição mesmo se o produto for removido.
    /// </summary>
    public string? ProductSnapshot { get; private set; } // JSON
    
    public DateTime CreatedAt { get; private set; }

    private UserFavorite() { }

    /// <summary>
    /// Cria um novo favorito.
    /// </summary>
    public static UserFavorite Create(Guid userId, Guid productId, string? productSnapshot = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

        return new UserFavorite
        {
            UserId = userId,
            ProductId = productId,
            ProductSnapshot = productSnapshot,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Atualiza o snapshot do produto.
    /// </summary>
    public void UpdateSnapshot(string snapshot)
    {
        ProductSnapshot = snapshot;
    }
}
