using BuildingBlocks.Domain.Entities;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Reserva de Estoque.
/// Corresponde à tabela catalog.stock_reservations no banco de dados.
/// </summary>
public class StockReservation : Entity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public string ReferenceType { get; private set; } = null!;
    public Guid ReferenceId { get; private set; }
    public int Quantity { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Indica se a reserva está ativa (não expirada e não liberada).
    /// </summary>
    public bool IsActive => ReleasedAt == null && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Indica se a reserva expirou.
    /// </summary>
    public bool IsExpired => ReleasedAt == null && ExpiresAt <= DateTime.UtcNow;

    private StockReservation() { }

    internal StockReservation(
        Guid productId,
        string referenceType,
        Guid referenceId,
        int quantity,
        DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(referenceType))
            throw new ArgumentException("Reference type cannot be empty.", nameof(referenceType));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        ProductId = productId;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Quantity = quantity;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Libera a reserva de estoque.
    /// </summary>
    internal void Release()
    {
        if (ReleasedAt != null)
            throw new InvalidOperationException("Reservation already released.");

        ReleasedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Estende a expiração da reserva.
    /// </summary>
    public void ExtendExpiration(TimeSpan duration)
    {
        if (ReleasedAt != null)
            throw new InvalidOperationException("Cannot extend a released reservation.");

        ExpiresAt = DateTime.UtcNow.Add(duration);
    }
}
