using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Domain.Entities;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Fornece identidade única via Guid e suporte a eventos de domínio.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Identificador único da entidade.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Eventos de domínio levantados pela entidade.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Entity Id cannot be empty.", nameof(id));

        Id = id;
    }

    /// <summary>
    /// Adiciona um evento de domínio à entidade.
    /// </summary>
    /// <param name="domainEvent">Evento a ser adicionado</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove um evento de domínio específico.
    /// </summary>
    /// <param name="domainEvent">Evento a ser removido</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Limpa todos os eventos de domínio da entidade.
    /// Usado após a publicação dos eventos pelo Event Dispatcher.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    #region Equality

    public bool Equals(Entity? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity && Equals(entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }

    #endregion
}