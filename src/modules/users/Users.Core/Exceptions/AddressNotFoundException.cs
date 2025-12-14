using BuildingBlocks.Domain.Exceptions;

namespace Users.Core.Exceptions;

/// <summary>
/// Exception thrown when an address is not found.
/// </summary>
public sealed class AddressNotFoundException : EntityNotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddressNotFoundException"/> class.
    /// </summary>
    /// <param name="addressId">The ID of the address that was not found</param>
    public AddressNotFoundException(Guid addressId)
        : base($"Address with ID '{addressId}' was not found.")
    {
        AddressId = addressId;
    }

    /// <summary>
    /// Gets the address ID that was not found.
    /// </summary>
    public Guid AddressId { get; }
}
