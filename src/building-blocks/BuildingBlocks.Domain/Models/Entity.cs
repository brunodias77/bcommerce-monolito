namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Classe base para todas as entidades do domínio
/// Uma entidade é identificada por seu ID, não por seus atributos
/// Duas entidades são consideradas iguais se possuem o mesmo ID
/// </summary>
/// <typeparam name="TId">Tipo do identificador da entidade</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    public TId Id { get; protected set; }

    protected Entity(TId id)
    {
        Id = id;
    }

    // Construtor protegido sem parâmetros para EF Core
    protected Entity()
    {
        Id = default!;
    }

    #region Equality

    /// <summary>
    /// Verifica se duas entidades são iguais comparando seus IDs
    /// </summary>
    public bool Equals(Entity<TId>? other)
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

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Verifica se duas entidades são iguais
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    /// <summary>
    /// Obtém o hash code baseado no ID da entidade
    /// </summary>
    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    /// <summary>
    /// Operador de igualdade
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
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
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    #endregion
}
