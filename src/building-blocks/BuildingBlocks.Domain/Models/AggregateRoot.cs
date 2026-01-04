using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Classe base para raízes de agregado no DDD
/// Agregados são clusters de objetos de domínio que devem ser tratados como uma unidade
/// A raiz do agregado é responsável por:
/// - Manter a consistência interna do agregado
/// - Gerenciar eventos de domínio que ocorrem dentro do agregado
/// - Ser o único ponto de entrada para modificações no agregado
/// </summary>
/// <typeparam name="TId">Tipo do identificador da entidade</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Lista de eventos de domínio que ocorreram neste agregado
    /// Eventos são publicados após a persistência bem-sucedida (padrão Outbox)
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    // Construtor protegido sem parâmetros para EF Core
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Adiciona um evento de domínio à lista de eventos pendentes
    /// O evento será publicado após o SaveChanges ser chamado com sucesso
    /// </summary>
    /// <param name="domainEvent">Evento de domínio a ser adicionado</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Limpa todos os eventos de domínio pendentes
    /// Chamado automaticamente após os eventos serem convertidos em mensagens de outbox
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Classe base para raízes de agregado com ID do tipo Guid
/// Versão conveniente para o caso mais comum (ID = Guid)
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot() : base()
    {
    }
}
