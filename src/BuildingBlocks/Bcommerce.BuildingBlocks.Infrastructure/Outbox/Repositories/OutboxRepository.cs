using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Repositories;

public class OutboxRepository(BaseDbContext dbContext) : IOutboxRepository
{
    private readonly BaseDbContext _dbContext = dbContext;

    public async Task AddAsync(OutboxMessage message)
    {
        await _dbContext.Set<OutboxMessage>().AddAsync(message);
    }
}
