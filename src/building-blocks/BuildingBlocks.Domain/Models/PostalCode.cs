using System.Text.RegularExpressions;

namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Value Object representing a Brazilian Postal Code (CEP - Código de Endereçamento Postal).
/// Validates format and normalizes the value.
/// </summary>
/// <remarks>
/// CEP format: XXXXX-XXX (5 digits, dash, 3 digits)
/// Can be created from formatted (00000-000) or unformatted (00000000) strings.
/// </remarks>
/// <example>
/// <code>
/// var postalCode = PostalCode.Create("01310-100");
/// Console.WriteLine(postalCode.Value); // "01310-100"
/// Console.WriteLine(postalCode.UnformattedValue); // "01310100"
///
/// var postalCode2 = PostalCode.Create("01310100"); // Also works
/// Console.WriteLine(postalCode2.Value); // "01310-100"
/// </code>
/// </example>
public sealed class PostalCode : ValueObject
{
    private static readonly Regex FormattedRegex = new(@"^\d{5}-\d{3}$", RegexOptions.Compiled);
    private static readonly Regex UnformattedRegex = new(@"^\d{8}$", RegexOptions.Compiled);

    /// <summary>
    /// Gets the formatted postal code value (XXXXX-XXX).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the unformatted postal code value (only digits).
    /// </summary>
    public string UnformattedValue { get; }

    private PostalCode(string value, string unformattedValue)
    {
        Value = value;
        UnformattedValue = unformattedValue;
    }

    /// <summary>
    /// Creates a new PostalCode instance with validation.
    /// Accepts both formatted (XXXXX-XXX) and unformatted (XXXXXXXX) values.
    /// </summary>
    /// <param name="value">The postal code to validate</param>
    /// <returns>A PostalCode instance</returns>
    /// <exception cref="ArgumentException">Thrown when postal code is invalid</exception>
    public static PostalCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Postal code cannot be null or empty.", nameof(value));

        value = value.Trim();

        string digitsOnly;

        // Check if it's already formatted
        if (FormattedRegex.IsMatch(value))
        {
            digitsOnly = value.Replace("-", string.Empty);
        }
        // Check if it's unformatted
        else if (UnformattedRegex.IsMatch(value))
        {
            digitsOnly = value;
        }
        else
        {
            throw new ArgumentException(
                "Postal code must be in format XXXXX-XXX or XXXXXXXX (8 digits).",
                nameof(value)
            );
        }

        // Validate it's not all zeros
        if (digitsOnly == "00000000")
            throw new ArgumentException("Postal code cannot be 00000-000.", nameof(value));

        var formattedValue = FormatPostalCode(digitsOnly);

        return new PostalCode(formattedValue, digitsOnly);
    }

    /// <summary>
    /// Tries to create a PostalCode instance without throwing exceptions.
    /// </summary>
    /// <param name="value">The postal code to validate</param>
    /// <param name="postalCode">The created PostalCode if validation succeeds</param>
    /// <returns>True if postal code is valid, false otherwise</returns>
    public static bool TryCreate(string value, out PostalCode? postalCode)
    {
        try
        {
            postalCode = Create(value);
            return true;
        }
        catch
        {
            postalCode = null;
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is a valid postal code format.
    /// </summary>
    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim();

        return FormattedRegex.IsMatch(value) || UnformattedRegex.IsMatch(value);
    }

    /// <summary>
    /// Formats a postal code string to XXXXX-XXX pattern.
    /// </summary>
    private static string FormatPostalCode(string digitsOnly)
    {
        return $"{digitsOnly.Substring(0, 5)}-{digitsOnly.Substring(5, 3)}";
    }

    /// <summary>
    /// Implicit conversion from PostalCode to string.
    /// </summary>
    public static implicit operator string(PostalCode postalCode) => postalCode.Value;

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UnformattedValue;
    }
}
