using System.Collections.Concurrent;
using System.Reflection;

namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Classe base para criar enumerações com comportamento (Smart Enums).
/// Substitui enums C# tradicionais quando você precisa de lógica adicional.
/// </summary>
/// <remarks>
/// No seu schema PostgreSQL, você tem vários ENUMs nativos:
/// - shared.order_status, shared.payment_status, shared.coupon_status, etc.
///
/// Esta classe permite criar versões C# desses enums com métodos e validações.
///
/// Performance:
/// - GetAll() usa cache interno para evitar reflection repetida
/// - FromId() e FromName() também são otimizados via cache
///
/// Exemplo de uso:
/// <code>
/// public class OrderStatus : Enumeration
/// {
///     public static OrderStatus Pending = new(1, "PENDING");
///     public static OrderStatus Paid = new(2, "PAID");
///     public static OrderStatus Shipped = new(3, "SHIPPED");
///     public static OrderStatus Delivered = new(4, "DELIVERED");
///     public static OrderStatus Cancelled = new(5, "CANCELLED");
///
///     private OrderStatus(int id, string name) : base(id, name) { }
///
///     public bool CanBeCancelled() => this == Pending || this == Paid;
///     public bool IsCompleted() => this == Delivered || this == Cancelled;
/// }
/// </code>
/// </remarks>
public abstract class Enumeration : IComparable
{
    // Cache para valores de enumeração (evita reflection repetida)
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<Enumeration>> _valuesCache = new();

    // Cache para lookup por Id
    private static readonly ConcurrentDictionary<Type, Dictionary<int, Enumeration>> _idCache = new();

    // Cache para lookup por Nome (case-insensitive)
    private static readonly ConcurrentDictionary<Type, Dictionary<string, Enumeration>> _nameCache = new();

    /// <summary>
    /// Valor numérico da enumeração.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Nome da enumeração (geralmente em UPPER_CASE para match com PostgreSQL).
    /// </summary>
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    /// <summary>
    /// Retorna todos os valores definidos de um tipo específico de enumeração.
    /// </summary>
    /// <remarks>
    /// Os valores são cacheados após a primeira chamada para melhor performance.
    /// </remarks>
    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var values = GetCachedValues(typeof(T));
        return values.Cast<T>();
    }

    /// <summary>
    /// Obtém um valor específico pelo Id.
    /// </summary>
    /// <remarks>
    /// Usa lookup O(1) via cache de dicionário.
    /// </remarks>
    public static T? FromId<T>(int id) where T : Enumeration
    {
        var idLookup = GetIdLookup(typeof(T));
        return idLookup.TryGetValue(id, out var value) ? (T)value : null;
    }

    /// <summary>
    /// Obtém um valor específico pelo Nome (case-insensitive).
    /// </summary>
    /// <remarks>
    /// Usa lookup O(1) via cache de dicionário.
    /// </remarks>
    public static T? FromName<T>(string name) where T : Enumeration
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var nameLookup = GetNameLookup(typeof(T));
        return nameLookup.TryGetValue(name.ToUpperInvariant(), out var value) ? (T)value : null;
    }

    /// <summary>
    /// Tenta obter um valor específico pelo Id.
    /// </summary>
    public static bool TryFromId<T>(int id, out T? enumeration) where T : Enumeration
    {
        enumeration = FromId<T>(id);
        return enumeration != null;
    }

    /// <summary>
    /// Tenta obter um valor específico pelo Nome.
    /// </summary>
    public static bool TryFromName<T>(string name, out T? enumeration) where T : Enumeration
    {
        enumeration = FromName<T>(name);
        return enumeration != null;
    }

    /// <summary>
    /// Obtém os valores cacheados para um tipo de enumeração.
    /// </summary>
    private static IReadOnlyList<Enumeration> GetCachedValues(Type type)
    {
        return _valuesCache.GetOrAdd(type, t =>
        {
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            var values = fields
                .Select(f => f.GetValue(null))
                .OfType<Enumeration>()
                .ToList();

            return values.AsReadOnly();
        });
    }

    /// <summary>
    /// Obtém o lookup por Id cacheado para um tipo de enumeração.
    /// </summary>
    private static Dictionary<int, Enumeration> GetIdLookup(Type type)
    {
        return _idCache.GetOrAdd(type, _ =>
        {
            var values = GetCachedValues(type);
            return values.ToDictionary(e => e.Id);
        });
    }

    /// <summary>
    /// Obtém o lookup por Nome cacheado para um tipo de enumeração (case-insensitive).
    /// </summary>
    private static Dictionary<string, Enumeration> GetNameLookup(Type type)
    {
        return _nameCache.GetOrAdd(type, _ =>
        {
            var values = GetCachedValues(type);
            return values.ToDictionary(
                e => e.Name.ToUpperInvariant(),
                StringComparer.OrdinalIgnoreCase);
        });
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
            return false;

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public int CompareTo(object? other)
    {
        if (other is null)
            return 1;

        if (other is not Enumeration otherEnumeration)
            throw new ArgumentException($"Object must be of type {nameof(Enumeration)}");

        return Id.CompareTo(otherEnumeration.Id);
    }

    public static bool operator ==(Enumeration? left, Enumeration? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Enumeration? left, Enumeration? right)
    {
        return !(left == right);
    }

    public static bool operator <(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) >= 0;
    }
}


