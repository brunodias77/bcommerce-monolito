using BuildingBlocks.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Users.Presentation.Controllers;

/// <summary>
/// Controller para operações de sessões.
/// </summary>
[Authorize]
public class SessionsController : ApiControllerBase
{
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ILogger<SessionsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista as sessões ativas do usuário.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetActiveSessionsQuery via MediatR
        // var result = await _mediator.Send(new GetActiveSessionsQuery(userId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetActiveSessionsQuery não implementado" });
    }

    /// <summary>
    /// Revoga uma sessão específica.
    /// </summary>
    [HttpDelete("{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Revogando sessão {SessionId} para UserId: {UserId}", sessionId, userId);

        // TODO: Enviar RevokeSessionCommand via MediatR
        // var result = await _mediator.Send(new RevokeSessionCommand(userId, sessionId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "RevokeSessionCommand não implementado" });
    }

    /// <summary>
    /// Revoga todas as outras sessões (mantém apenas a atual).
    /// </summary>
    [HttpPost("revoke-others")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeOtherSessions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var currentSessionId = GetCurrentSessionId();

        _logger.LogInformation("Revogando outras sessões para UserId: {UserId}", userId);

        // TODO: Enviar RevokeOtherSessionsCommand via MediatR
        // var result = await _mediator.Send(new RevokeOtherSessionsCommand(userId, currentSessionId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "RevokeOtherSessionsCommand não implementado" });
    }

    /// <summary>
    /// Busca o histórico de login do usuário.
    /// </summary>
    [HttpGet("login-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLoginHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetLoginHistoryQuery via MediatR
        // var result = await _mediator.Send(new GetLoginHistoryQuery(userId, page, pageSize), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetLoginHistoryQuery não implementado" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value 
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) 
            ? userId 
            : throw new UnauthorizedAccessException("UserId não encontrado no token");
    }

    private Guid? GetCurrentSessionId()
    {
        var sessionIdClaim = User.FindFirst("session_id")?.Value;
        return Guid.TryParse(sessionIdClaim, out var sessionId) ? sessionId : null;
    }
}
