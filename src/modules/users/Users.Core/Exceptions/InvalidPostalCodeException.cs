using BuildingBlocks.Domain.Exceptions;

namespace Users.Core.Exceptions;

/// <summary>
/// Exception thrown when a postal code (CEP) is invalid.
/// </summary>
public sealed class InvalidPostalCodeException : BusinessRuleValidationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPostalCodeException"/> class.
    /// </summary>
    /// <param name="postalCode">The invalid postal code value</param>
    public InvalidPostalCodeException(string postalCode)
        : base($"Postal code '{postalCode}' is invalid.")
    {
        PostalCode = postalCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPostalCodeException"/> class with a specific reason.
    /// </summary>
    /// <param name="postalCode">The invalid postal code value</param>
    /// <param name="reason">The specific reason why the postal code is invalid</param>
    public InvalidPostalCodeException(string postalCode, string reason)
        : base($"Postal code '{postalCode}' is invalid: {reason}")
    {
        PostalCode = postalCode;
        Reason = reason;
    }

    /// <summary>
    /// Gets the invalid postal code value.
    /// </summary>
    public string PostalCode { get; }

    /// <summary>
    /// Gets the specific reason why the postal code is invalid.
    /// </summary>
    public string? Reason { get; }
}
