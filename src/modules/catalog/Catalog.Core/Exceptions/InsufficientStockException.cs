using BuildingBlocks.Domain.Exceptions;

namespace Catalog.Core.Exceptions;

/// <summary>
/// Exceção lançada quando não há estoque suficiente para uma operação.
/// </summary>
public class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public int AvailableStock { get; }
    public int RequestedQuantity { get; }

    public InsufficientStockException(Guid productId, int availableStock, int requestedQuantity)
        : base($"Insufficient stock for product {productId}. Available: {availableStock}, Requested: {requestedQuantity}", 
               "INSUFFICIENT_STOCK")
    {
        ProductId = productId;
        AvailableStock = availableStock;
        RequestedQuantity = requestedQuantity;
    }

    public InsufficientStockException(string message)
        : base(message, "INSUFFICIENT_STOCK")
    {
    }
}
