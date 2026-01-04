using System.Reflection;

namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Classe base para Smart Enums (enumerações inteligentes)
///
/// Smart Enums são uma alternativa aos enums tradicionais do C# que oferecem:
/// - Métodos e comportamento
/// - Valores associados (não apenas inteiros)
/// - Conversão type-safe
/// - Validação de valores
///
/// Exemplos baseados no schema SQL:
/// - ProductStatus: Draft, Active, Inactive, OutOfStock, Discontinued
/// - OrderStatus: Pending, PaymentProcessing, Paid, Preparing, Shipped, etc.
/// - PaymentMethod: CreditCard, DebitCard, Pix, Boleto, Wallet, BankTransfer
/// </summary>
/// <typeparam name="TEnum">Tipo da enumeração (a própria classe derivada)</typeparam>
public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>, IComparable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Dictionary<int, TEnum> _enumerations = GetEnumerations();

    /// <summary>
    /// Valor numérico da enumeração
    /// </summary>
    public int Value { get; protected init; }

    /// <summary>
    /// Nome da enumeração
    /// </summary>
    public string Name { get; protected init; }

    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    /// <summary>
    /// Obtém todas as instâncias da enumeração
    /// </summary>
    public static IReadOnlyCollection<TEnum> GetAll()
    {
        return _enumerations.Values.ToList();
    }

    /// <summary>
    /// Obtém uma enumeração pelo valor numérico
    /// </summary>
    public static TEnum? FromValue(int value)
    {
        return _enumerations.TryGetValue(value, out var enumeration) ? enumeration : null;
    }

    /// <summary>
    /// Obtém uma enumeração pelo nome
    /// </summary>
    public static TEnum? FromName(string name)
    {
        return _enumerations.Values.FirstOrDefault(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tenta obter uma enumeração pelo valor numérico
    /// </summary>
    public static bool TryFromValue(int value, out TEnum? enumeration)
    {
        enumeration = FromValue(value);
        return enumeration is not null;
    }

    /// <summary>
    /// Tenta obter uma enumeração pelo nome
    /// </summary>
    public static bool TryFromName(string name, out TEnum? enumeration)
    {
        enumeration = FromName(name);
        return enumeration is not null;
    }

    /// <summary>
    /// Verifica se um valor numérico é válido
    /// </summary>
    public static bool IsValid(int value)
    {
        return _enumerations.ContainsKey(value);
    }

    /// <summary>
    /// Verifica se um nome é válido
    /// </summary>
    public static bool IsValid(string name)
    {
        return _enumerations.Values.Any(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public override string ToString() => Name;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj)
    {
        return obj is Enumeration<TEnum> other && Equals(other);
    }

    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null)
        {
            return false;
        }

        return GetType() == other.GetType() && Value == other.Value;
    }

    public int CompareTo(Enumeration<TEnum>? other)
    {
        return other is null ? 1 : Value.CompareTo(other.Value);
    }

    public static bool operator ==(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        return !(left == right);
    }

    public static bool operator <(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        return left is not null && left.CompareTo(right) < 0;
    }

    public static bool operator <=(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        return left is null || left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Obtém todas as instâncias da enumeração usando reflexão
    /// </summary>
    private static Dictionary<int, TEnum> GetEnumerations()
    {
        var enumerationType = typeof(TEnum);

        var fieldsForType = enumerationType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fieldInfo => enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo => (TEnum)fieldInfo.GetValue(null)!);

        return fieldsForType.ToDictionary(x => x.Value);
    }
}
