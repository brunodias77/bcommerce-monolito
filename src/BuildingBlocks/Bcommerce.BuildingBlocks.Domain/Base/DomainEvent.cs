using Bcommerce.BuildingBlocks.Domain.Abstractions;

namespace Bcommerce.BuildingBlocks.Domain.Base;

/// <summary>
/// Classe base abstrata para eventos de domínio.
/// </summary>
/// <remarks>
/// Fornece implementação padrão de IDomainEvent.
/// - EventId gerado automaticamente
/// - OccurredOn definido como DateTime.UtcNow
/// - EventType retorna o nome da classe
/// 
/// Exemplo de uso:
/// <code>
/// public class UsuarioCriadoEvent : DomainEvent
/// {
///     public Guid UsuarioId { get; }
///     public string Email { get; }
///     
///     public UsuarioCriadoEvent(Guid usuarioId, string email)
///     {
///         UsuarioId = usuarioId;
///         Email = email;
///     }
/// }
/// </code>
/// </remarks>
public abstract class DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();
    /// <inheritdoc />
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    /// <inheritdoc />
    public string EventType => GetType().Name;
}
