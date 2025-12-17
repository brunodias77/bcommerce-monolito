using Bcommerce.BuildingBlocks.Web.Controllers;
using Bcommerce.Modules.Users.Application.Commands.Login;
using Bcommerce.Modules.Users.Application.Commands.RefreshToken;
using Bcommerce.Modules.Users.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Modules.Users.Api.Controllers;

[AllowAnonymous]
public class AuthController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }
}
