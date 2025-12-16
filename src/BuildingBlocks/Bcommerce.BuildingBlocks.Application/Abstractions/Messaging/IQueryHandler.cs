using Bcommerce.BuildingBlocks.Application.Models;
using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
