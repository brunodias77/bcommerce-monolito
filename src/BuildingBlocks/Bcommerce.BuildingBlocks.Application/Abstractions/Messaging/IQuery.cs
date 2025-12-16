using Bcommerce.BuildingBlocks.Application.Models;
using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

// Opcional: IQuery sem generic se houver caso de uso sem retorno (raro para query)
public interface IQuery : IRequest<Result>
{
}
