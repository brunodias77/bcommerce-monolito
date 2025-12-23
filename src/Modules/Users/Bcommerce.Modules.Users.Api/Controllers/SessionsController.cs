using Bcommerce.BuildingBlocks.Web.Controllers;
using Bcommerce.Modules.Users.Application.Commands.RevokeSession;
using Bcommerce.Modules.Users.Application.DTOs;
using Bcommerce.Modules.Users.Application.Queries.GetActiveSessions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Modules.Users.Api.Controllers;

public class SessionsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SessionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken)
    {
        var query = new GetActiveSessionsQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }

    [HttpPost("revoke/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken cancellationToken)
    {
        var command = new RevokeSessionCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
