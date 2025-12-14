using BuildingBlocks.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Users.Presentation.Requests;

namespace Users.Presentation.Controllers;

/// <summary>
/// Controller para operações de usuários.
/// </summary>
public class UsersController : ApiControllerBase
{
    private readonly ILogger<UsersController> _logger;

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new { error = "As senhas não coincidem" });
        }

        _logger.LogInformation("Registrando novo usuário: {Email}", request.Email);

        // TODO: Enviar RegisterUserCommand via MediatR
        // var result = await _mediator.Send(new RegisterUserCommand(...), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "RegisterUserCommand não implementado" });
    }

    /// <summary>
    /// Busca o usuário atual.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetUserByIdQuery via MediatR
        // var result = await _mediator.Send(new GetUserByIdQuery(userId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetUserByIdQuery não implementado" });
    }

    /// <summary>
    /// Confirma o email do usuário.
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        // TODO: Enviar ConfirmEmailCommand via MediatR
        // var result = await _mediator.Send(new ConfirmEmailCommand(token), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "ConfirmEmailCommand não implementado" });
    }

    /// <summary>
    /// Altera a senha do usuário.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar ChangePasswordCommand via MediatR
        // var result = await _mediator.Send(new ChangePasswordCommand(userId, ...), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "ChangePasswordCommand não implementado" });
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

/// <summary>
/// Request para alteração de senha.
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);
