using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message);
}
