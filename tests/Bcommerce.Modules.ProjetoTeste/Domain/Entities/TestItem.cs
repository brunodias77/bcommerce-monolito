
using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.ProjetoTeste.Domain.Entities;

public class TestItem : AggregateRoot<Guid>
{
    // EF Core constructor
    protected TestItem() { } 

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    public bool IsActive { get; private set; }
    public decimal Value { get; private set; }

    public TestItem(string name, string description, decimal value)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Value = value;
        IsActive = true;

        AddDomainEvent(new Events.TestItemCreatedEvent(Id, Name));
    }

    public void UpdateName(string newName)
    {
        Name = newName;
        // Logic or events
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
