namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found in the database.
/// </summary>
/// <remarks>
/// This exception typically maps to HTTP 404 Not Found status code.
/// </remarks>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    public EntityNotFoundException()
        : base("The requested entity was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message</param>
    public EntityNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public EntityNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity</param>
    /// <param name="entityId">The ID of the entity</param>
    public EntityNotFoundException(string entityName, object entityId)
        : base($"Entity '{entityName}' with ID '{entityId}' was not found.")
    {
    }
}
