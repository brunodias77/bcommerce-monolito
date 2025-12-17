using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

namespace Bcommerce.BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Interface genérica para manipuladores de eventos de integração.
/// </summary>
/// <typeparam name="TIntegrationEvent">Tipo do evento de integração a ser manipulado.</typeparam>
/// <remarks>
/// Define o contrato para consumidores de mensagens.
/// - Implementa a lógica de reação a eventos externos
/// - Garante que o consumidor aceite apenas tipos válidos de IIntegrationEvent
/// 
/// Exemplo de uso:
/// <code>
/// public class OrderPlacedHandler : IIntegrationEventHandler&lt;OrderPlacedEvent&gt;
/// {
///     public async Task Handle(OrderPlacedEvent @event, CancellationToken ct) { ... }
/// }
/// </code>
/// </remarks>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    /// <summary>
    /// Processa o evento de integração recebido.
    /// </summary>
    /// <param name="event">Instância do evento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task Handle(TIntegrationEvent @event, CancellationToken cancellationToken);
}

/// <summary>
/// Interface marcadora para descoberta de handlers via Reflection.
/// </summary>
/// <remarks>
/// Facilita o registro automático de consumidores no container de DI.
/// - Usada para agrupar todos os handlers independente do tipo genérico
/// - Não contém métodos, serve apenas como constraint ou filtro
/// 
/// Exemplo de uso:
/// <code>
/// services.Scan(s => s.FromAssemblies(...)
///     .AddClasses(c => c.AssignableTo&lt;IIntegrationEventHandler&gt;())
///     ...);
/// </code>
/// </remarks>
public interface IIntegrationEventHandler
{
}
