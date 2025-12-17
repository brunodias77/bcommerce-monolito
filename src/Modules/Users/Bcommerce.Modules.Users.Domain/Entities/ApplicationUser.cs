using Bcommerce.BuildingBlocks.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Bcommerce.Modules.Users.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, IAggregateRoot
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public ApplicationUser()
    {
        // IdentityUser requires a parameterless constructor
    }

    public ApplicationUser(string userName, string email) : base(userName)
    {
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
