using System.Text.RegularExpressions;

namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Value Object representing an email address.
/// Validates format and normalizes the value.
/// </summary>
/// <remarks>
/// Email validation follows a simplified RFC 5322 pattern.
/// The value is stored in lowercase for consistency.
/// </remarks>
/// <example>
/// <code>
/// var email = Email.Create("user@example.com");
/// Console.WriteLine(email.Value); // "user@example.com"
/// Console.WriteLine(email.Domain); // "example.com"
/// Console.WriteLine(email.LocalPart); // "user"
/// </code>
/// </example>
public sealed class Email : ValueObject
{
    // Simplified RFC 5322 email regex pattern
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Gets the email address value (normalized to lowercase).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the local part of the email (before @).
    /// </summary>
    public string LocalPart { get; }

    /// <summary>
    /// Gets the domain part of the email (after @).
    /// </summary>
    public string Domain { get; }

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
        var parts = Value.Split('@');
        LocalPart = parts[0];
        Domain = parts[1];
    }

    /// <summary>
    /// Creates a new Email instance with validation.
    /// </summary>
    /// <param name="value">The email address to validate</param>
    /// <returns>An Email instance</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid</exception>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty.", nameof(value));

        value = value.Trim();

        if (value.Length > 320) // Maximum email length per RFC 5321
            throw new ArgumentException("Email cannot exceed 320 characters.", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"Email '{value}' is not in a valid format.", nameof(value));

        var parts = value.Split('@');
        if (parts[0].Length > 64) // Maximum local part length
            throw new ArgumentException("Email local part cannot exceed 64 characters.", nameof(value));

        if (parts[1].Length > 255) // Maximum domain length
            throw new ArgumentException("Email domain cannot exceed 255 characters.", nameof(value));

        return new Email(value);
    }

    /// <summary>
    /// Tries to create an Email instance without throwing exceptions.
    /// </summary>
    /// <param name="value">The email address to validate</param>
    /// <param name="email">The created Email if validation succeeds</param>
    /// <returns>True if email is valid, false otherwise</returns>
    public static bool TryCreate(string value, out Email? email)
    {
        try
        {
            email = Create(value);
            return true;
        }
        catch
        {
            email = null;
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is a valid email format.
    /// </summary>
    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length > 320)
            return false;

        return EmailRegex.IsMatch(value);
    }

    /// <summary>
    /// Implicit conversion from Email to string.
    /// </summary>
    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
