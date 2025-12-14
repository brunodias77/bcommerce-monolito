using BuildingBlocks.Domain.Entities;

namespace Cart.Core.Entities;

/// <summary>
/// Item do carrinho de compras.
/// Corresponde à tabela cart.items no banco de dados.
/// </summary>
public class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }

    public string ProductSnapshot { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal? CurrentPrice { get; private set; }
    public DateTime? PriceChangedAt { get; private set; }

    public bool StockReserved { get; private set; }
    public Guid? StockReservationId { get; private set; }

    public DateTime AddedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? RemovedAt { get; private set; }

    private CartItem()
    {
    }

    public static CartItem Create(
        Guid cartId,
        Guid productId,
        string productSnapshot,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = productId,
            ProductSnapshot = productSnapshot,
            Quantity = quantity,
            UnitPrice = unitPrice,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRemoved()
    {
        RemovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePriceIfChanged(decimal currentPrice)
    {
        if (currentPrice != UnitPrice)
        {
            CurrentPrice = currentPrice;
            PriceChangedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetStockReservation(Guid reservationId)
    {
        StockReservationId = reservationId;
        StockReserved = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseStockReservation()
    {
        StockReservationId = null;
        StockReserved = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
