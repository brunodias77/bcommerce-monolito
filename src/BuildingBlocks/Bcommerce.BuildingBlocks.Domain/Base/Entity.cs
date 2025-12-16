using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Base;

public abstract class Entity<TId> : IEntity
{
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
