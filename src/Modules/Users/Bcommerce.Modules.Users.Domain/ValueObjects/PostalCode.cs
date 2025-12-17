using System.Text.RegularExpressions;
using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.ValueObjects;

public class PostalCode : ValueObject
{
    public string Value { get; }

    // Formato brasileiro: 00000-000 ou 00000000
    private static readonly Regex PostalCodeRegex = new(@"^\d{5}-?\d{3}$", RegexOptions.Compiled);

    private PostalCode(string value)
    {
        Value = value;
    }

    public static PostalCode Create(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            throw new ArgumentException("Postal code cannot be empty.", nameof(postalCode));
        }

        if (!PostalCodeRegex.IsMatch(postalCode))
        {
            throw new ArgumentException("Invalid postal code format.", nameof(postalCode));
        }
        
        return new PostalCode(postalCode);
    }
    
    // Método para formatar output se necessário
    public string Formatted() 
    {
        if (Value.Contains('-')) return Value;
        return $"{Value[..5]}-{Value[5..]}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
