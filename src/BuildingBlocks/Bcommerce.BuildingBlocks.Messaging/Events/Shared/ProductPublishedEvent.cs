namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

/// <summary>
/// Evento disparado quando um novo produto é publicado ou atualizado no catálogo.
/// </summary>
/// <remarks>
/// Sincroniza dados de produtos entre serviços.
/// - Pode atualizar réplicas de leitura ou índices de busca (ElasticSearch)
/// - Mantém consistência eventual do catálogo
/// 
/// Exemplo de uso:
/// <code>
/// new ProductPublishedEvent(prod.Id, prod.Name, prod.Price, prod.Sku);
/// </code>
/// </remarks>
public record ProductPublishedEvent(Guid ProductId, string Name, decimal Price, string Sku) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
