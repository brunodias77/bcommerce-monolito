using Bcommerce.BuildingBlocks.Application.Models;
using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Contrato para manipuladores de consultas.
/// </summary>
/// <typeparam name="TQuery">Tipo da query a ser processada.</typeparam>
/// <typeparam name="TResponse">Tipo do dado retornado pela query.</typeparam>
/// <remarks>
/// Encapsula a lógica de leitura de dados.
/// - Deve ser livre de efeitos colaterais
/// - Pode acessar repositórios de leitura ou banco diretamente (Dapper)
/// - Retorna Result contendo os dados ou erro de validação
/// 
/// Exemplo de uso:
/// <code>
/// public class ObterProdutoHandler : IQueryHandler&lt;ObterProdutoQuery, ProdutoDto&gt;
/// {
///     public async Task&lt;Result&lt;ProdutoDto&gt;&gt; Handle(ObterProdutoQuery q, CancellationToken ct)
///     {
///         var dto = await _repo.GetByIdAsync(q.Id);
///         return Result.Success(dto);
///     }
/// }
/// </code>
/// </remarks>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
