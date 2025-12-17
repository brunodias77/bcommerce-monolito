
using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.ProjetoTeste.Domain.Events;

public class TestItemCreatedEvent : DomainEvent
{
    public Guid TestItemId { get; }
    public string Name { get; }

    public TestItemCreatedEvent(Guid testItemId, string name)
    {
        TestItemId = testItemId;
        Name = name;
    }
}
