using Bcommerce.BuildingBlocks.Web.Controllers;
using Bcommerce.Modules.Users.Application.Commands.MarkNotificationAsRead;
using Bcommerce.Modules.Users.Application.Commands.UpdateNotificationPreferences;
using Bcommerce.Modules.Users.Application.DTOs;
using Bcommerce.Modules.Users.Application.Queries.GetNotificationPreferences;
using Bcommerce.Modules.Users.Application.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Modules.Users.Api.Controllers;

public class NotificationsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationsQuery(unreadOnly);
        var result = await _sender.Send(query, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkNotificationAsReadCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpGet("preferences")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var query = new GetNotificationPreferencesQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }

    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result);
    }
}
