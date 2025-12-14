namespace BuildingBlocks.Domain.Entities;

/// <summary>
/// Classe base para Aggregate Roots no padrão DDD.
/// Adiciona versionamento para Optimistic Concurrency Control.
/// </summary>
/// <remarks>
/// No seu schema PostgreSQL, a coluna 'version' é atualizada via trigger:
/// - CREATE TRIGGER trg_xxx_version BEFORE UPDATE ... EXECUTE FUNCTION shared.trigger_increment_version()
/// </remarks>
public abstract class AggregateRoot : Entity
{
    /// <summary>
    /// Versão da entidade para controle de concorrência otimista.
    /// Incrementada automaticamente via trigger no PostgreSQL.
    /// </summary>
    public int Version { get; protected set; }

    protected AggregateRoot() : base()
    {
        Version = 1;
    }

    protected AggregateRoot(Guid id) : base(id)
    {
        Version = 1;
    }

    /// <summary>
    /// Incrementa a versão manualmente (caso não use trigger no banco).
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }
}