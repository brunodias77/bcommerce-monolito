namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Classe base para objetos de valor (Value Objects) no DDD
///
/// Objetos de valor são definidos por seus atributos, não por identidade
/// Características:
/// - Imutáveis (não podem ser modificados após criação)
/// - Igualdade estrutural (dois objetos são iguais se todos os atributos são iguais)
/// - Não possuem identidade própria
///
/// Exemplos: Endereço, Dinheiro, Período, Email, CPF
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Retorna os componentes atômicos que definem a igualdade deste objeto de valor
    /// Cada classe derivada deve implementar este método retornando todos os valores
    /// que determinam a igualdade do objeto
    /// </summary>
    /// <returns>Componentes atômicos do objeto</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Verifica se dois objetos de valor são iguais comparando todos os seus componentes
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Verifica se dois objetos são iguais
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is ValueObject valueObject && Equals(valueObject);
    }

    /// <summary>
    /// Calcula o hash code baseado em todos os componentes do objeto de valor
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Operador de igualdade
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
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

    /// <summary>
    /// Operador de desigualdade
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Cria uma cópia do objeto de valor
    /// Como objetos de valor são imutáveis, retorna o próprio objeto
    /// </summary>
    public ValueObject Copy()
    {
        return (ValueObject)MemberwiseClone();
    }
}
