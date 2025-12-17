using Bcommerce.BuildingBlocks.Web.Controllers;
using Bcommerce.Modules.Users.Application.Commands.UpdateProfile;
using Bcommerce.Modules.Users.Application.DTOs;
using Bcommerce.Modules.Users.Application.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Modules.Users.Api.Controllers;

public class ProfilesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        // Assuming GetUserProfileQuery might use ICurrentUserService internally or take an ID
        // If it takes an ID, we'd extract it from the User claims.
        // specific implementation depends on Query definition. Assuming parameterless implies "current user" or takes ID.
        // Let's assume it takes an ID for safer design, extracted from claims.
        // But for now, sticking to a generic signature or assuming the Query handles "current user" context logic if no ID is passed
        // OR standard pattern: query takes UserId.
        
        // However, since I cannot see the Application layer, I will assume a standard pattern where I pass nothing if the handler uses CurrentUserService,
        // or I pass a property from 'User' (ClaimsPrincipal).
        // Let's assume the query requires a userId.
        
        // var userId = User.GetUserId(); // Assuming extension method exists or similar mechanism
        // For compilation safety without visible extensions, I will assume the Query constructor might be empty and Handle uses context,
        // OR I simply instantiate it.
        
        var query = new GetUserProfileQuery(); 
        var result = await _sender.Send(query, cancellationToken);
        
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }
}
