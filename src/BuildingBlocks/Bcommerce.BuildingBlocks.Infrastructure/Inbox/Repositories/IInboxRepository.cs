using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Repositories;

public interface IInboxRepository
{
    Task AddAsync(InboxMessage message);
}
