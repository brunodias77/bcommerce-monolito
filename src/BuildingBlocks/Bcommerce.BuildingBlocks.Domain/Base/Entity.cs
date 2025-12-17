using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Base;

/// <summary>
/// Classe base abstrata para todas as entidades de domínio.
/// </summary>
/// <typeparam name="TId">Tipo do identificador (ex: Guid, int).</typeparam>
/// <remarks>
/// Implementa igualdade por identidade e rastreamento de timestamps.
/// - Comparação por Id (não por referência)
/// - Operators == e != sobrecarregados
/// - CreatedAt definido automaticamente no construtor
/// 
/// Exemplo de uso:
/// <code>
/// public class Categoria : Entity&lt;Guid&gt;
/// {
///     public string Nome { get; private set; }
///     
///     public Categoria(string nome) : base(Guid.NewGuid())
///     {
///         Nome = nome;
///     }
/// }
/// </code>
/// </remarks>
public abstract class Entity<TId> : IEntity
{
    /// <summary>Identificador único da entidade.</summary>
    public TId Id { get; protected set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected Entity() { }
    
    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> item) return false;

        if (ReferenceEquals(this, item)) return true;

        if (GetType() != item.GetType()) return false;

        if (Id == null || item.Id == null) return false;

        return Id.Equals(item.Id);
    }

    public override int GetHashCode()
    {
        return (GetType().GetHashCode() * 907) + Id.GetHashCode();
    }

    public static bool operator ==(Entity<TId> left, Entity<TId> right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId> left, Entity<TId> right)
    {
        return !(left == right);
    }
}
