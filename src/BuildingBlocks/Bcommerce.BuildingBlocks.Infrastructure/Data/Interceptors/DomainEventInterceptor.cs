using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;
using Bcommerce.BuildingBlocks.Domain.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor para disparo de eventos de domínio (Síncrono).
/// </summary>
/// <remarks>
/// Publica eventos armazenados nas entidades logo após o commit.
/// - Alternativa simples ao Outbox Pattern
/// - Dispara eventos APÓS SaveChangesAsync ter sucesso
/// - Limpa os eventos da entidade após publicação
/// 
/// Exemplo de uso:
/// <code>
/// // Útil para testes ou cenários sem necessidade de consistência forte de mensageria
/// optionsBuilder.AddInterceptors(domainEventInterceptor);
/// </code>
/// </remarks>
public class DomainEventInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    private readonly IPublisher _publisher = publisher;

    // NOTA: Em uma arquitetura com Outbox, este interceptor pode não ser necessário 
    // se o despacho de eventos for feito via OutboxProcessor.
    // Ou ele pode servir para disparar eventos de domínio que devem acontecer na mesma transação (o que é raro no DDD puro, mas útil).
    // Alternativamente, ele pode ser usado para ENFILEIRAR eventos no Outbox (implementação seria diferente).
    // A implementação abaixo DISPARA os eventos imediatamente ANTES ou DEPOIS do SaveChanges.
    
    // Para simplificação neste momento, vamos apenas publicar os eventos APÓS o save changes ter sucesso.
    // Mas para consistência eventual real, deve-se usar o OutboxPattern implementado separadamente.
    
    // Vou comentar a publicação direta para não conflitar com o Outbox, mas manter a classe caso,
    // o usuário queira usar eventos síncronos em memória.

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEvents(DbContext? context)
    {
        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent);
        }
    }
}
