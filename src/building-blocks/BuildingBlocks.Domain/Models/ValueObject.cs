namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Classe base para Value Objects no padrão DDD.
/// Fornece igualdade baseada em valores ao invés de identidade.
/// </summary>
/// <remarks>
/// Value Objects são imutáveis e não possuem identidade própria.
/// Dois Value Objects são iguais se todos os seus valores componentes são iguais.
/// 
/// Exemplo de uso:
/// <code>
/// public class Address : ValueObject
/// {
///     public string Street { get; }
///     public string City { get; }
///     public string PostalCode { get; }
///     
///     public Address(string street, string city, string postalCode)
///     {
///         Street = street;
///         City = city;
///         PostalCode = postalCode;
///     }
///     
///     protected override IEnumerable&lt;object&gt; GetEqualityComponents()
///     {
///         yield return Street;
///         yield return City;
///         yield return PostalCode;
///     }
/// }
/// </code>
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Retorna os componentes que definem a igualdade do Value Object.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && Equals(other);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Where(x => x != null)
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + obj!.GetHashCode();
                }
            });
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Cria uma cópia profunda do Value Object.
    /// </summary>
    protected virtual ValueObject GetCopy()
    {
        return (ValueObject)MemberwiseClone();
    }
}


