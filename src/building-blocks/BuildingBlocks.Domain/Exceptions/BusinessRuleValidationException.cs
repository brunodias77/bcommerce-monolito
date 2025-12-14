namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule validation fails.
/// </summary>
/// <remarks>
/// This exception typically maps to HTTP 422 Unprocessable Entity or 400 Bad Request status code.
/// </remarks>
public class BusinessRuleValidationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class.
    /// </summary>
    public BusinessRuleValidationException()
        : base("A business rule validation failed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message</param>
    public BusinessRuleValidationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public BusinessRuleValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code for localization or client handling</param>
    public BusinessRuleValidationException(string message, string errorCode)
        : base(message, errorCode)
    {
    }
}
