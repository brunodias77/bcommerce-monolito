using Bcommerce.BuildingBlocks.Domain.Abstractions;
using MediatR;

namespace Bcommerce.BuildingBlocks.Domain.Events;

/// <summary>
/// Contrato para handlers de eventos de domínio.
/// </summary>
/// <typeparam name="TDomainEvent">Tipo do evento a ser tratado.</typeparam>
/// <remarks>
/// Encapsula MediatR.INotificationHandler para semântica DDD.
/// - Um evento pode ter múltiplos handlers
/// - Handlers executam após persistência (eventual consistency)
/// - Use para side effects e notificações
/// 
/// Exemplo de uso:
/// <code>
/// public class EnviarEmailHandler : IDomainEventHandler&lt;UsuarioCriadoEvent&gt;
/// {
///     private readonly IEmailService _emailService;
///     
///     public async Task Handle(UsuarioCriadoEvent evento, CancellationToken ct)
///     {
///         await _emailService.EnviarBoasVindas(evento.Email);
///     }
/// }
/// </code>
/// </remarks>
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
}
