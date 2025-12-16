using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Repositories;

public class InboxRepository(BaseDbContext dbContext) : IInboxRepository
{
    private readonly BaseDbContext _dbContext = dbContext;

    public async Task AddAsync(InboxMessage message)
    {
        await _dbContext.Set<InboxMessage>().AddAsync(message);
    }
}
