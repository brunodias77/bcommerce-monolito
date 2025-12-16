using Bcommerce.BuildingBlocks.Domain.Abstractions;
using MediatR;

namespace Bcommerce.BuildingBlocks.Domain.Events;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
}
