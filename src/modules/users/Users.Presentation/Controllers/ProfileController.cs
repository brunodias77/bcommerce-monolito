using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Users.Presentation.Requests;

namespace Users.Presentation.Controllers;

/// <summary>
/// Controller para operações de perfil.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IMediator mediator,
        ILogger<ProfileController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Busca o perfil do usuário atual.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetUserProfileQuery via MediatR
        // var result = await _mediator.Send(new GetUserProfileQuery(userId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetUserProfileQuery não implementado" });
    }

    /// <summary>
    /// Cria o perfil do usuário.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Criando perfil para UserId: {UserId}", userId);

        // TODO: Enviar CreateProfileCommand via MediatR
        // var result = await _mediator.Send(new CreateProfileCommand(userId, ...), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "CreateProfileCommand não implementado" });
    }

    /// <summary>
    /// Atualiza o perfil do usuário.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Atualizando perfil para UserId: {UserId}", userId);

        // TODO: Enviar UpdateProfileCommand via MediatR
        // var result = await _mediator.Send(new UpdateProfileCommand(userId, ...), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "UpdateProfileCommand não implementado" });
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
