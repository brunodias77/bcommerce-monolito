using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Repositories;

/// <summary>
/// Implementação do repositório de Inbox usando EF Core.
/// </summary>
/// <remarks>
/// Persiste mensagens na tabela InboxMessages através do BaseDbContext.
/// - Utiliza AddAsync do DbSet
/// - Não chama SaveChanges automaticamente (controle via UnitOfWork externo)
/// 
/// Exemplo de uso:
/// <code>
/// // Injeção via DI
/// services.AddScoped&lt;IInboxRepository, InboxRepository&gt;();
/// </code>
/// </remarks>
public class InboxRepository(BaseDbContext dbContext) : IInboxRepository
{
    private readonly BaseDbContext _dbContext = dbContext;

    /// <inheritdoc />
    public async Task AddAsync(InboxMessage message)
    {
        await _dbContext.Set<InboxMessage>().AddAsync(message);
    }
}
