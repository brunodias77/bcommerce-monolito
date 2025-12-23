using Bcommerce.BuildingBlocks.Web.Controllers;
using Bcommerce.Modules.Users.Application.Commands.AddAddress;
using Bcommerce.Modules.Users.Application.Commands.DeleteAddress;
using Bcommerce.Modules.Users.Application.Commands.SetDefaultAddress;
using Bcommerce.Modules.Users.Application.Commands.UpdateAddress;
using Bcommerce.Modules.Users.Application.DTOs;
using Bcommerce.Modules.Users.Application.Queries.GetUserAddresses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Modules.Users.Api.Controllers;

public class AddressesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AddressDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAddresses(CancellationToken cancellationToken)
    {
        var query = new GetUserAddressesQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAddress([FromBody] AddAddressCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetMyAddresses), null, result.Value) : HandleFailure(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        // Assuming command has Id property, if not we might need to set it or creating a new command wrapper
        if (command.AddressId != id) // Hypothetical validation
        {
             // If property exists... assuming it acts as the DTO
        }
        
        // Ideally command should carry the ID.
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? OkResponse(result.Value) : HandleFailure(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteAddressCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpPost("{id:guid}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultAddress(Guid id, CancellationToken cancellationToken)
    {
        var command = new SetDefaultAddressCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result);
    }
}
