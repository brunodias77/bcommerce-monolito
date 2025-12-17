using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Base;

/// <summary>
/// Classe base abstrata para Value Objects no padrão DDD.
/// </summary>
/// <remarks>
/// Value Objects são comparados por valor, não por referência.
/// - Override GetEqualityComponents() para definir atributos de igualdade
/// - Equals e GetHashCode implementados automaticamente
/// - Imutáveis por design
/// 
/// Exemplo de uso:
/// <code>
/// public class Endereco : ValueObject
/// {
///     public string Rua { get; }
///     public string Cidade { get; }
///     
///     public Endereco(string rua, string cidade)
///     {
///         Rua = rua;
///         Cidade = cidade;
///     }
///     
///     protected override IEnumerable&lt;object&gt; GetEqualityComponents()
///     {
///         yield return Rua;
///         yield return Cidade;
///     }
/// }
/// </code>
/// </remarks>
public abstract class ValueObject : IValueObject
{
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }
        return ReferenceEquals(left, null) || left.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    /// <summary>
    /// Retorna os componentes usados para comparação de igualdade.
    /// Deve ser implementado nas classes derivadas.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}
