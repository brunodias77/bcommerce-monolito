using BuildingBlocks.Domain.Exceptions;

namespace Users.Core.Exceptions;

/// <summary>
/// Exception thrown when a session is not found.
/// </summary>
public sealed class SessionNotFoundException : EntityNotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionNotFoundException"/> class.
    /// </summary>
    /// <param name="sessionId">The ID of the session that was not found</param>
    public SessionNotFoundException(Guid sessionId)
        : base($"Session with ID '{sessionId}' was not found.")
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// Gets the session ID that was not found.
    /// </summary>
    public Guid SessionId { get; }
}
