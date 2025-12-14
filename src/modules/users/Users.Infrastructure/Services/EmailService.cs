using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Services;

/// <summary>
/// Interface para serviço de envio de emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia um email.
    /// </summary>
    Task SendEmailAsync(
        string to, 
        string subject, 
        string body, 
        bool isHtml = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia email de confirmação de conta.
    /// </summary>
    Task SendEmailConfirmationAsync(
        string to, 
        string userName, 
        string confirmationLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia email de recuperação de senha.
    /// </summary>
    Task SendPasswordResetAsync(
        string to, 
        string userName, 
        string resetLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia email de boas-vindas.
    /// </summary>
    Task SendWelcomeEmailAsync(
        string to, 
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia email de alerta de segurança.
    /// </summary>
    Task SendSecurityAlertAsync(
        string to, 
        string userName, 
        string alertType, 
        string details,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do serviço de email usando SMTP.
/// Em produção, substituir por SendGrid, AWS SES, etc.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(
        ILogger<EmailService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string to, 
        string subject, 
        string body, 
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implementar envio real via SMTP ou serviço de email
        _logger.LogInformation(
            "Enviando email para {To} com assunto '{Subject}'",
            to,
            subject);

        // Simulação de delay de envio
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation(
            "Email enviado com sucesso para {To}",
            to);
    }

    public async Task SendEmailConfirmationAsync(
        string to, 
        string userName, 
        string confirmationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Confirme seu email - BCommerce";
        var body = $@"
            <h2>Olá {userName}!</h2>
            <p>Obrigado por se cadastrar na BCommerce.</p>
            <p>Por favor, clique no link abaixo para confirmar seu email:</p>
            <p><a href='{confirmationLink}'>Confirmar Email</a></p>
            <p>Se você não criou uma conta, ignore este email.</p>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        string to, 
        string userName, 
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Redefinir sua senha - BCommerce";
        var body = $@"
            <h2>Olá {userName}!</h2>
            <p>Recebemos uma solicitação para redefinir sua senha.</p>
            <p>Clique no link abaixo para criar uma nova senha:</p>
            <p><a href='{resetLink}'>Redefinir Senha</a></p>
            <p>Este link expira em 24 horas.</p>
            <p>Se você não solicitou a redefinição, ignore este email.</p>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(
        string to, 
        string userName,
        CancellationToken cancellationToken = default)
    {
        var subject = "Bem-vindo à BCommerce!";
        var body = $@"
            <h2>Bem-vindo, {userName}!</h2>
            <p>Sua conta foi criada com sucesso.</p>
            <p>Comece a explorar nossos produtos e aproveite as melhores ofertas!</p>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendSecurityAlertAsync(
        string to, 
        string userName, 
        string alertType, 
        string details,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Alerta de Segurança - {alertType}";
        var body = $@"
            <h2>Olá {userName}!</h2>
            <p>Detectamos uma atividade suspeita em sua conta:</p>
            <p><strong>{alertType}</strong></p>
            <p>{details}</p>
            <p>Se não foi você, recomendamos alterar sua senha imediatamente.</p>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}
