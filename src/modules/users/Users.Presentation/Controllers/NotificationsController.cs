using BuildingBlocks.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Users.Presentation.Controllers;

/// <summary>
/// Controller para operações de notificações.
/// </summary>
[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(ILogger<NotificationsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista as notificações do usuário.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetNotificationsQuery via MediatR
        // var result = await _mediator.Send(new GetNotificationsQuery(userId, page, pageSize, unreadOnly), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetNotificationsQuery não implementado" });
    }

    /// <summary>
    /// Busca a contagem de notificações não lidas.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetUnreadNotificationsCountQuery via MediatR
        // var result = await _mediator.Send(new GetUnreadNotificationsCountQuery(userId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetUnreadNotificationsCountQuery não implementado" });
    }

    /// <summary>
    /// Marca notificações como lidas.
    /// </summary>
    [HttpPost("mark-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkNotificationsAsReadRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Marcando {Count} notificações como lidas para UserId: {UserId}", 
            request.NotificationIds.Count, userId);

        // TODO: Enviar MarkNotificationsAsReadCommand via MediatR
        // var result = await _mediator.Send(new MarkNotificationsAsReadCommand(userId, request.NotificationIds), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "MarkNotificationsAsReadCommand não implementado" });
    }

    /// <summary>
    /// Marca todas as notificações como lidas.
    /// </summary>
    [HttpPost("mark-all-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Marcando todas as notificações como lidas para UserId: {UserId}", userId);

        // TODO: Enviar MarkAllNotificationsAsReadCommand via MediatR
        // var result = await _mediator.Send(new MarkAllNotificationsAsReadCommand(userId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "MarkAllNotificationsAsReadCommand não implementado" });
    }

    /// <summary>
    /// Busca as preferências de notificação do usuário.
    /// </summary>
    [HttpGet("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        // TODO: Enviar GetNotificationPreferencesQuery via MediatR
        // var result = await _mediator.Send(new GetNotificationPreferencesQuery(userId), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "GetNotificationPreferencesQuery não implementado" });
    }

    /// <summary>
    /// Atualiza as preferências de notificação.
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation("Atualizando preferências de notificação para UserId: {UserId}", userId);

        // TODO: Enviar UpdateNotificationPreferencesCommand via MediatR
        // var result = await _mediator.Send(new UpdateNotificationPreferencesCommand(userId, ...), cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "UpdateNotificationPreferencesCommand não implementado" });
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
/// Request para marcar notificações como lidas.
/// </summary>
public record MarkNotificationsAsReadRequest(List<Guid> NotificationIds);

/// <summary>
/// Request para atualizar preferências de notificação.
/// </summary>
public record UpdateNotificationPreferencesRequest(
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool OrderUpdates,
    bool PromotionalEmails,
    bool SecurityAlerts
);
