using BuildingBlocks.Domain.Exceptions;

namespace Users.Core.Exceptions;

/// <summary>
/// Exception thrown when an email address is invalid.
/// </summary>
public sealed class InvalidEmailException : BusinessRuleValidationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidEmailException"/> class.
    /// </summary>
    /// <param name="email">The invalid email value</param>
    public InvalidEmailException(string email)
        : base($"Email '{email}' is invalid.")
    {
        Email = email;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidEmailException"/> class with a specific reason.
    /// </summary>
    /// <param name="email">The invalid email value</param>
    /// <param name="reason">The specific reason why the email is invalid</param>
    public InvalidEmailException(string email, string reason)
        : base($"Email '{email}' is invalid: {reason}")
    {
        Email = email;
        Reason = reason;
    }

    /// <summary>
    /// Gets the invalid email value.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the specific reason why the email is invalid.
    /// </summary>
    public string? Reason { get; }
}
