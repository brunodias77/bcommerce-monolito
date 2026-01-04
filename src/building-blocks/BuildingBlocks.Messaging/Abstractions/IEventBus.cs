namespace BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Interface para barramento de eventos (Event Bus)
/// Abstrai a publicação e assinatura de eventos de integração entre módulos
///
/// Implementações disponíveis:
/// - InMemoryEventBus: Para desenvolvimento e testes (sem infraestrutura externa)
/// - MassTransitEventBus: Para produção usando RabbitMQ (mensageria robusta)
///
/// Padrão Pub/Sub:
/// - Publishers (Publicadores): Módulos que PUBLICAM eventos quando algo acontece
/// - Subscribers (Assinantes): Módulos que CONSOMEM eventos e reagem a eles
///
/// Exemplo de fluxo baseado no schema SQL:
///
/// 1. Usuário finaliza checkout do carrinho
///    → Módulo Cart converte carrinho em pedido
///    → Cart publica: PedidoCriadoIntegrationEvent
///
/// 2. Módulo Orders consome o evento
///    → Cria registro em orders.orders
///    → Cria itens em orders.items
///    → Publica: PedidoCriadoParaPagamentoIntegrationEvent
///
/// 3. Módulo Payments consome o evento
///    → Processa pagamento com gateway
///    → Cria registro em payments.payments
///    → Publica: PagamentoAprovadoIntegrationEvent (se sucesso)
///    → OU: PagamentoFalhouIntegrationEvent (se falha)
///
/// 4. Módulo Orders consome PagamentoAprovadoIntegrationEvent
///    → Atualiza orders.orders (status = PAID, paid_at = NOW())
///    → Cria histórico em orders.status_history
///    → Publica: PedidoPagoIntegrationEvent
///
/// 5. Módulo Catalog consome PedidoPagoIntegrationEvent
///    → Confirma reservas em catalog.stock_reservations
///    → Atualiza estoque em catalog.products (stock -= quantidade)
///    → Cria movimentação em catalog.stock_movements
///
/// 6. Módulo Notifications consome PedidoPagoIntegrationEvent
///    → Cria notificação em users.notifications
///    → Envia email de confirmação
///
/// Vantagens deste padrão:
/// - Baixo acoplamento: Módulos não se conhecem diretamente
/// - Escalabilidade: Fácil adicionar novos consumidores
/// - Resiliência: Retry automático em caso de falha
/// - Auditoria: Histórico completo de eventos
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica um evento de integração para todos os assinantes
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento (deve implementar IIntegrationEvent)</typeparam>
    /// <param name="event">Evento a ser publicado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;

    /// <summary>
    /// Registra um assinante para um tipo específico de evento
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <typeparam name="THandler">Tipo do handler que processará o evento</typeparam>
    /// <remarks>
    /// Em MassTransit, isso é feito automaticamente via configuração
    /// Em InMemory, mantém um dicionário de handlers
    /// </remarks>
    void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;

    /// <summary>
    /// Remove um assinante de um tipo específico de evento
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento</typeparam>
    /// <typeparam name="THandler">Tipo do handler a ser removido</typeparam>
    void Unsubscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
}

/// <summary>
/// Interface para handlers de eventos de integração
/// Cada módulo implementa handlers para os eventos que deseja consumir
/// </summary>
/// <typeparam name="TEvent">Tipo do evento que este handler processa</typeparam>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : class, IIntegrationEvent
{
    /// <summary>
    /// Processa o evento de integração
    /// </summary>
    /// <param name="event">Evento a ser processado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando o processamento</returns>
    /// <remarks>
    /// Implementações devem ser idempotentes (podem ser chamadas múltiplas vezes com o mesmo evento)
    /// Use InboxInterceptor para garantir que o evento não seja processado duplicadamente
    /// </remarks>
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}
