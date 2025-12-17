using Bcommerce.BuildingBlocks.Web.Controllers;
using Bcommerce.Modules.Users.Application.Commands.RegisterUser;
using Bcommerce.Modules.Users.Application.DTOs;
using Bcommerce.Modules.Users.Application.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Modules.Users.Api.Controllers;

public class UsersController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }
}
