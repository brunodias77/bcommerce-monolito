using System.Reflection;

namespace Bcommerce.BuildingBlocks.Domain.Base;

/// <summary>
/// Classe base para enumerações tipadas (Smart Enum pattern).
/// </summary>
/// <remarks>
/// Alternativa type-safe a enums padrão do C#.
/// - Suporta comportamento e propriedades adicionais
/// - Serializável como int (Id) ou string (Name)
/// - GetAll() retorna todos os valores definidos
/// 
/// Exemplo de uso:
/// <code>
/// public class StatusPedido : Enumeration
/// {
///     public static readonly StatusPedido Pendente = new(1, "Pendente");
///     public static readonly StatusPedido Aprovado = new(2, "Aprovado");
///     public static readonly StatusPedido Cancelado = new(3, "Cancelado");
///     
///     private StatusPedido(int id, string name) : base(id, name) { }
/// }
/// 
/// // Uso:
/// var todos = Enumeration.GetAll&lt;StatusPedido&gt;();
/// </code>
/// </remarks>
public abstract class Enumeration : IComparable
{
    /// <summary>Nome legível da enumeração.</summary>
    public string Name { get; private set; }
    /// <summary>Identificador numérico da enumeração.</summary>
    public int Id { get; private set; }

    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    public override string ToString() => Name;

    /// <summary>Retorna todas as instâncias definidas do tipo de enumeração.</summary>
    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
                 .Select(f => f.GetValue(null))
                 .Cast<T>();

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? other) => Id.CompareTo(((Enumeration?)other)?.Id);
}
