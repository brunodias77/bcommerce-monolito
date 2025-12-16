using Bcommerce.BuildingBlocks.Application.Models;
using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
