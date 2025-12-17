using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Repositories;

/// <summary>
/// Implementação do repositório de Outbox via EF Core.
/// </summary>
/// <remarks>
/// Persiste mensagens na tabela OutboxMessages.
/// - Adiciona ao ChangeTracker do EF Core
/// - Commit é feito pela transação principal da aplicação
/// 
/// Exemplo de uso:
/// <code>
/// services.AddScoped&lt;IOutboxRepository, OutboxRepository&gt;();
/// </code>
/// </remarks>
public class OutboxRepository(BaseDbContext dbContext) : IOutboxRepository
{
    private readonly BaseDbContext _dbContext = dbContext;

    /// <inheritdoc />
    public async Task AddAsync(OutboxMessage message)
    {
        await _dbContext.Set<OutboxMessage>().AddAsync(message);
    }
}
