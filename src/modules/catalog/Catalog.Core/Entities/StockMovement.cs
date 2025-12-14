using BuildingBlocks.Domain.Entities;
using Catalog.Core.Enums;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Movimentação de Estoque.
/// Corresponde à tabela catalog.stock_movements no banco de dados.
/// </summary>
public class StockMovement : Entity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public StockMovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    
    public int StockBefore { get; private set; }
    public int StockAfter { get; private set; }
    
    public string? Reason { get; private set; }
    public Guid? PerformedBy { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    internal StockMovement(
        Guid productId,
        StockMovementType movementType,
        int quantity,
        string? referenceType,
        Guid? referenceId,
        int stockBefore,
        int stockAfter,
        string? reason = null,
        Guid? performedBy = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        ProductId = productId;
        MovementType = movementType;
        Quantity = quantity;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        StockBefore = stockBefore;
        StockAfter = stockAfter;
        Reason = reason;
        PerformedBy = performedBy;
        CreatedAt = DateTime.UtcNow;
    }
}
