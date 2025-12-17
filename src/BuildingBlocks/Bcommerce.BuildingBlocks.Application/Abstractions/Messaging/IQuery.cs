using Bcommerce.BuildingBlocks.Application.Models;
using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Contrato para consultas que retornam dados (Read Side).
/// </summary>
/// <typeparam name="TResponse">Tipo do dado retornado.</typeparam>
/// <remarks>
/// Representa uma intenção de leitura de dados sem efeitos colaterais.
/// - Implementa IRequest&lt;Result&lt;TResponse&gt;&gt; do MediatR
/// - Retorno sempre encapsulado em Result&lt;T&gt;
/// - Otimizado para leitura (pode usar Dapper/Projections)
/// 
/// Exemplo de uso:
/// <code>
/// public record ObterProdutoPorIdQuery(Guid Id) : IQuery&lt;ProdutoDto&gt;;
/// </code>
/// </remarks>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

// Opcional: IQuery sem generic se houver caso de uso sem retorno (raro para query)
/// <summary>
/// Contrato para consultas sem retorno de dados (apenas verificação/ping).
/// </summary>
/// <remarks>
/// Caso raro, usado para verificações de saúde ou existência ("dry run").
/// - Implementa IRequest&lt;Result&gt;
/// - Prefira IQuery&lt;T&gt; para a maioria dos casos
/// 
/// Exemplo de uso:
/// <code>
/// public record HealthCheckQuery() : IQuery;
/// </code>
/// </remarks>
public interface IQuery : IRequest<Result>
{
}
