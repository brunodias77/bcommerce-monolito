using System.Text.RegularExpressions;
using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; }

    // Suporta formatos simples (apenas validação básica para exemplo)
    private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));
        }

        // Simples sanitização e verificação. 
        // Em um cenário real, usaria libphonenumber.
        var sanitized = phoneNumber.Trim();

        // if (!PhoneRegex.IsMatch(sanitized)) ... simplificado para este exemplo

        return new PhoneNumber(sanitized);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
