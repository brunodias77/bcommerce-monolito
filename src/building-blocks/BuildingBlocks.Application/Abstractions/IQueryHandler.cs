using BuildingBlocks.Application.Results;
using MediatR;

namespace BuildingBlocks.Application.Abstractions;

/// <summary>
/// Handler para queries.
/// </summary>
/// <remarks>
/// O QueryHandler é responsável por buscar os dados de forma performática.
/// Diferente dos CommandHandlers, aqui podemos:
/// - Usar AsNoTracking() do EF Core
/// - Usar Dapper para consultas SQL diretas (Raw SQL)
/// - Retornar DTOs/ViewModels diretamente (projecão) em vez de entidades de domínio completas
/// </remarks>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}