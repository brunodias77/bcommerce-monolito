using BuildingBlocks.Application.Results;
using MediatR;

namespace BuildingBlocks.Application.Abstractions;

/// <summary>
/// Handler para queries.
/// </summary>
/// <typeparam name="TQuery">Tipo da query</typeparam>
/// <typeparam name="TResponse">Tipo do valor retornado</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}