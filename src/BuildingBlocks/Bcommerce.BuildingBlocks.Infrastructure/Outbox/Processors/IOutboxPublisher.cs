using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Processors;

// Interface usada pelo OutboxInterceptor para persistir mensagens
public interface IOutboxPublisher
{
    // A implementação real usaria o IOutboxRepository dentro do escopo da transação
    // Mas o interceptor já tem acesso ao DbContext. 
    // Vamos criar apenas para formalizar se necessário, 
    // mas na prática o Interceptor fará o trabalho direto no DbContext.
}
