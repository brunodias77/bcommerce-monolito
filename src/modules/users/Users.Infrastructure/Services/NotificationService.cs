using Microsoft.Extensions.Logging;
using Users.Core.Entities;
using Users.Core.Repositories;

namespace Users.Infrastructure.Services;

/// <summary>
/// Interface para serviço de notificações in-app.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Envia uma notificação para o usuário.
    /// </summary>
    Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        string notificationType,
        string? referenceType = null,
        Guid? referenceId = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca notificações como lidas.
    /// </summary>
    Task MarkAsReadAsync(
        Guid userId,
        IEnumerable<Guid> notificationIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca todas as notificações do usuário como lidas.
    /// </summary>
    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia notificação de boas-vindas.
    /// </summary>
    Task SendWelcomeNotificationAsync(
        Guid userId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia notificação de alerta de segurança.
    /// </summary>
    Task SendSecurityAlertNotificationAsync(
        Guid userId,
        string alertType,
        string details,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do serviço de notificações in-app.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferencesRepository _preferencesRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPreferencesRepository preferencesRepository,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _preferencesRepository = preferencesRepository;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        string notificationType,
        string? referenceType = null,
        Guid? referenceId = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        // Criar notificação in-app
        var notification = new Notification(
            userId,
            title,
            message,
            notificationType,
            referenceType,
            referenceId,
            actionUrl);

        await _notificationRepository.AddAsync(notification, cancellationToken);

        _logger.LogInformation(
            "Notificação '{NotificationType}' criada para UserId: {UserId}",
            notificationType,
            userId);

        // Verificar preferências do usuário para notificações adicionais
        var preferences = await _preferencesRepository.GetByUserIdAsync(userId, cancellationToken);

        if (preferences?.PushEnabled == true)
        {
            // TODO: Implementar push notification
            _logger.LogInformation(
                "Push notification seria enviada para UserId: {UserId}",
                userId);
        }
    }

    public async Task MarkAsReadAsync(
        Guid userId,
        IEnumerable<Guid> notificationIds,
        CancellationToken cancellationToken = default)
    {
        foreach (var notificationId in notificationIds)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
            
            if (notification is not null && notification.UserId == userId)
            {
                notification.MarkAsRead();
            }
        }

        _logger.LogInformation(
            "Marcadas {Count} notificações como lidas para UserId: {UserId}",
            notificationIds.Count(),
            userId);
    }

    public async Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _notificationRepository
            .GetUnreadByUserIdAsync(userId, cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        _logger.LogInformation(
            "Todas as notificações marcadas como lidas para UserId: {UserId}",
            userId);
    }

    public async Task SendWelcomeNotificationAsync(
        Guid userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        await SendNotificationAsync(
            userId,
            "Bem-vindo à BCommerce!",
            $"Olá {userName}! Sua conta foi criada com sucesso. Explore nossos produtos e aproveite!",
            "WELCOME",
            actionUrl: "/",
            cancellationToken: cancellationToken);
    }

    public async Task SendSecurityAlertNotificationAsync(
        Guid userId,
        string alertType,
        string details,
        CancellationToken cancellationToken = default)
    {
        await SendNotificationAsync(
            userId,
            $"Alerta de Segurança: {alertType}",
            details,
            "SECURITY_ALERT",
            actionUrl: "/account/security",
            cancellationToken: cancellationToken);
    }
}
