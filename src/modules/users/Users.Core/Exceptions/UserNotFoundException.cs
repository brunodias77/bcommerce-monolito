using BuildingBlocks.Domain.Exceptions;

namespace Users.Core.Exceptions;

/// <summary>
/// Exception thrown when a user is not found.
/// </summary>
public sealed class UserNotFoundException : EntityNotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotFoundException"/> class.
    /// </summary>
    /// <param name="userId">The ID of the user that was not found</param>
    public UserNotFoundException(Guid userId)
        : base($"User with ID '{userId}' was not found.")
    {
        UserId = userId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotFoundException"/> class.
    /// </summary>
    /// <param name="email">The email of the user that was not found</param>
    public UserNotFoundException(string email)
        : base($"User with email '{email}' was not found.")
    {
        Email = email;
    }

    /// <summary>
    /// Gets the user ID that was not found.
    /// </summary>
    public Guid? UserId { get; }

    /// <summary>
    /// Gets the email that was not found.
    /// </summary>
    public string? Email { get; }
}
