using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Users.Presentation.Requests;

namespace Users.Presentation.Controllers;

/// <summary>
/// Controller para operações de endereços.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(
        IMediator mediator,
        ILogger<AddressesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista os endereços do usuário.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetUserAddressesQuery via MediatR
        // var result = await _mediator.Send(new GetUserAddressesQuery(userId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetUserAddressesQuery não implementado" });
    }

    /// <summary>
    /// Busca um endereço por ID.
    /// </summary>
    [HttpGet("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddress(
        Guid addressId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetAddressByIdQuery via MediatR
        // var result = await _mediator.Send(new GetAddressByIdQuery(userId, addressId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetAddressByIdQuery não implementado" });
    }

    /// <summary>
    /// Adiciona um novo endereço.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddAddress(
        [FromBody] AddAddressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Adicionando endereço para UserId: {UserId}", userId);

        // TODO: Enviar AddAddressCommand via MediatR
        // var result = await _mediator.Send(new AddAddressCommand(userId, ...), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "AddAddressCommand não implementado" });
    }

    /// <summary>
    /// Atualiza um endereço existente.
    /// </summary>
    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(
        Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Atualizando endereço {AddressId} para UserId: {UserId}", addressId, userId);

        // TODO: Enviar UpdateAddressCommand via MediatR
        // var result = await _mediator.Send(new UpdateAddressCommand(userId, addressId, ...), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "UpdateAddressCommand não implementado" });
    }

    /// <summary>
    /// Remove um endereço (soft delete).
    /// </summary>
    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(
        Guid addressId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Removendo endereço {AddressId} para UserId: {UserId}", addressId, userId);

        // TODO: Enviar DeleteAddressCommand via MediatR
        // var result = await _mediator.Send(new DeleteAddressCommand(userId, addressId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "DeleteAddressCommand não implementado" });
    }

    /// <summary>
    /// Define um endereço como padrão.
    /// </summary>
    [HttpPost("{addressId:guid}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultAddress(
        Guid addressId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Definindo endereço {AddressId} como padrão para UserId: {UserId}", addressId, userId);

        // TODO: Enviar SetDefaultAddressCommand via MediatR
        // var result = await _mediator.Send(new SetDefaultAddressCommand(userId, addressId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "SetDefaultAddressCommand não implementado" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value 
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) 
            ? userId 
            : throw new UnauthorizedAccessException("UserId não encontrado no token");
    }
}
