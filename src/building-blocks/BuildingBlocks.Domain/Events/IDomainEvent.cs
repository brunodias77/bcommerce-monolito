using MediatR;

namespace BuildingBlocks.Domain.Events;

/// <summary>
/// Interface para eventos de domínio
/// Eventos de domínio representam algo que aconteceu no domínio e que outras partes do sistema podem estar interessadas
/// Exemplos: ProdutoCriadoEvent, PedidoFinalizadoEvent, PagamentoAprovadoEvent
///
/// Herda de INotification do MediatR para permitir o padrão de notificação/publicação
/// Eventos são processados de forma assíncrona após a persistência no banco (via Outbox)
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Identificador único do evento
    /// Usado para rastreamento e garantir idempotência no processamento
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Data e hora em que o evento ocorreu (UTC)
    /// </summary>
    DateTime OccurredAt { get; }
}