using BuildingBlocks.Domain.Exceptions;

namespace Catalog.Core.Exceptions;

/// <summary>
/// Exceção lançada quando um preço inválido é fornecido.
/// </summary>
public class InvalidPriceException : DomainException
{
    public InvalidPriceException(string message)
        : base(message, "INVALID_PRICE")
    {
    }

    public InvalidPriceException(decimal price)
        : base($"Invalid price: {price}. Price must be greater than or equal to zero.", "INVALID_PRICE")
    {
    }
}
