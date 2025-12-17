namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Marca uma entidade como raiz de agregado no padrão DDD.
/// </summary>
/// <remarks>
/// Aggregate Root é a única entidade acessível externamente em um agregado.
/// - Mantém consistência das invariantes do agregado
/// - Coleta eventos de domínio para publicação após persistência
/// - Única entidade com repositório próprio
/// 
/// Exemplo de uso:
/// <code>
/// public class Pedido : AggregateRoot&lt;Guid&gt;
/// {
///     public void AdicionarItem(ProdutoId produtoId, int quantidade)
///     {
///         var item = new ItemPedido(produtoId, quantidade);
///         _itens.Add(item);
///         AddDomainEvent(new ItemAdicionadoEvent(Id, produtoId));
///     }
/// }
/// </code>
/// </remarks>
public interface IAggregateRoot : IEntity
{
    /// <summary>Coleção de eventos de domínio pendentes.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    /// <summary>Limpa eventos após dispatch (chamado pela infraestrutura).</summary>
    void ClearDomainEvents();
}
