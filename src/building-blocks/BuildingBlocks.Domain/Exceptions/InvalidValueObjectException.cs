namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exception thrown when a Value Object cannot be created due to invalid data.
/// </summary>
/// <remarks>
/// This exception typically maps to HTTP 400 Bad Request status code.
/// </remarks>
public class InvalidValueObjectException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidValueObjectException"/> class.
    /// </summary>
    public InvalidValueObjectException()
        : base("The provided value object data is invalid.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidValueObjectException"/> class.
    /// </summary>
    /// <param name="message">The error message</param>
    public InvalidValueObjectException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidValueObjectException"/> class.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public InvalidValueObjectException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidValueObjectException"/> class.
    /// </summary>
    /// <param name="valueObjectName">The name of the value object</param>
    /// <param name="value">The invalid value</param>
    public InvalidValueObjectException(string valueObjectName, object value)
        : base($"Value '{value}' is not valid for '{valueObjectName}'.")
    {
    }
}
