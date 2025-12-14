using System.Text.RegularExpressions;

namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Value Object representing a Brazilian CPF (Cadastro de Pessoa Física).
/// Validates both format and check digits.
/// </summary>
/// <remarks>
/// CPF format: XXX.XXX.XXX-XX (with dots and dash)
/// This implementation validates:
/// - Format (11 digits with proper punctuation)
/// - Check digits (two verification digits)
/// - Rejects known invalid patterns (000.000.000-00, 111.111.111-11, etc.)
/// </remarks>
/// <example>
/// <code>
/// var cpfResult = Cpf.Create("123.456.789-09");
/// if (cpfResult.IsSuccess)
/// {
///     var cpf = cpfResult.Value;
///     Console.WriteLine(cpf.Value); // "123.456.789-09"
///     Console.WriteLine(cpf.UnformattedValue); // "12345678909"
/// }
/// </code>
/// </example>
public sealed class Cpf : ValueObject
{
    private static readonly Regex CpfFormatRegex = new(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", RegexOptions.Compiled);

    /// <summary>
    /// Gets the formatted CPF value (XXX.XXX.XXX-XX).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the unformatted CPF value (only digits).
    /// </summary>
    public string UnformattedValue { get; }

    private Cpf(string value, string unformattedValue)
    {
        Value = value;
        UnformattedValue = unformattedValue;
    }

    /// <summary>
    /// Creates a new CPF instance with validation.
    /// </summary>
    /// <param name="value">The CPF value to validate (can be formatted or unformatted)</param>
    /// <returns>A Result containing the CPF or validation errors</returns>
    public static Cpf Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CPF cannot be null or empty.", nameof(value));

        // Remove formatting if present
        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());

        // Validate length
        if (digitsOnly.Length != 11)
            throw new ArgumentException("CPF must contain exactly 11 digits.", nameof(value));

        // Validate known invalid patterns (all same digits)
        if (digitsOnly.Distinct().Count() == 1)
            throw new ArgumentException("CPF cannot have all identical digits.", nameof(value));

        // Validate check digits
        if (!IsValidCheckDigits(digitsOnly))
            throw new ArgumentException("CPF has invalid check digits.", nameof(value));

        // Format the CPF
        var formattedValue = FormatCpf(digitsOnly);

        return new Cpf(formattedValue, digitsOnly);
    }

    /// <summary>
    /// Tries to create a CPF instance without throwing exceptions.
    /// </summary>
    /// <param name="value">The CPF value to validate</param>
    /// <param name="cpf">The created CPF if validation succeeds</param>
    /// <returns>True if CPF is valid, false otherwise</returns>
    public static bool TryCreate(string value, out Cpf? cpf)
    {
        try
        {
            cpf = Create(value);
            return true;
        }
        catch
        {
            cpf = null;
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is a valid CPF format.
    /// </summary>
    public static bool IsValidFormat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return CpfFormatRegex.IsMatch(value);
    }

    /// <summary>
    /// Validates the CPF check digits using the official algorithm.
    /// </summary>
    private static bool IsValidCheckDigits(string digitsOnly)
    {
        // Calculate first check digit
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += int.Parse(digitsOnly[i].ToString()) * (10 - i);
        }
        int remainder = sum % 11;
        int firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if (int.Parse(digitsOnly[9].ToString()) != firstDigit)
            return false;

        // Calculate second check digit
        sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += int.Parse(digitsOnly[i].ToString()) * (11 - i);
        }
        remainder = sum % 11;
        int secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return int.Parse(digitsOnly[10].ToString()) == secondDigit;
    }

    /// <summary>
    /// Formats a CPF string to XXX.XXX.XXX-XX pattern.
    /// </summary>
    private static string FormatCpf(string digitsOnly)
    {
        return $"{digitsOnly.Substring(0, 3)}.{digitsOnly.Substring(3, 3)}.{digitsOnly.Substring(6, 3)}-{digitsOnly.Substring(9, 2)}";
    }

    /// <summary>
    /// Implicit conversion from Cpf to string.
    /// </summary>
    public static implicit operator string(Cpf cpf) => cpf.Value;

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UnformattedValue;
    }
}
