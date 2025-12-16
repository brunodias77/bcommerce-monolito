using BuildingBlocks.Application.Results;
using MediatR;

namespace BuildingBlocks.Application.Abstractions;

/// <summary>
/// Interface para queries (read operations) no padrão CQRS.
/// Queries não modificam estado e sempre retornam dados.
/// </summary>
/// <typeparam name="TResponse">Tipo do valor retornado</typeparam>
/// <remarks>
/// Queries são usadas para:
/// - Buscar entidades por Id
/// - Listar entidades com filtros
/// - Obter dados agregados
/// - Executar buscas complexas
/// 
/// IMPORTANTE: Queries NÃO devem modificar o estado do sistema.
/// 
/// Exemplo de uso:
/// <code>
/// public record GetProductByIdQuery(Guid ProductId) : IQuery&lt;ProductDto&gt;;
/// 
/// internal class GetProductByIdQueryHandler : IQueryHandler&lt;GetProductByIdQuery, ProductDto&gt;
/// {
///     private readonly IProductRepository _repository;
///     
///     public async Task&lt;Result&lt;ProductDto&gt;&gt; Handle(GetProductByIdQuery query, CancellationToken ct)
///     {
///         var product = await _repository.GetByIdAsync(query.ProductId, ct);
///         
///         if (product == null)
///             return Result.Fail&lt;ProductDto&gt;("Product not found", "PRODUCT_NOT_FOUND");
///         
///         var dto = ProductDto.FromEntity(product);
///         return Result.Ok(dto);
///     }
/// }
/// 
/// // Query com paginação
/// public record SearchProductsQuery(
///     string SearchTerm,
///     Guid? CategoryId,
///     int PageNumber,
///     int PageSize
/// ) : IQuery&lt;PagedResult&lt;ProductDto&gt;&gt;;
/// </code>
/// </remarks>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
    // Queries representam operações SÓ DE LEITURA (Read-Only).
    // Elas NÃO devem causar efeitos colaterais (side-effects) no sistema.
    // Usamos Result<TResponse> para padronizar o retorno, inclusive para casos de "Not Found".
}