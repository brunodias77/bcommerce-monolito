using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Services;

/// <summary>
/// Interface para serviço de envio de SMS.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Envia um SMS.
    /// </summary>
    Task SendSmsAsync(
        string phoneNumber, 
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia código de verificação por SMS.
    /// </summary>
    Task SendVerificationCodeAsync(
        string phoneNumber, 
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia alerta de segurança por SMS.
    /// </summary>
    Task SendSecurityAlertAsync(
        string phoneNumber, 
        string alertType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do serviço de SMS.
/// Em produção, substituir por Twilio, AWS SNS, etc.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(
        ILogger<SmsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendSmsAsync(
        string phoneNumber, 
        string message,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implementar envio real via Twilio ou outro serviço
        _logger.LogInformation(
            "Enviando SMS para {PhoneNumber}: '{Message}'",
            MaskPhoneNumber(phoneNumber),
            message);

        // Simulação de delay de envio
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation(
            "SMS enviado com sucesso para {PhoneNumber}",
            MaskPhoneNumber(phoneNumber));
    }

    public async Task SendVerificationCodeAsync(
        string phoneNumber, 
        string code,
        CancellationToken cancellationToken = default)
    {
        var message = $"BCommerce: Seu código de verificação é {code}. Válido por 10 minutos.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task SendSecurityAlertAsync(
        string phoneNumber, 
        string alertType,
        CancellationToken cancellationToken = default)
    {
        var message = $"BCommerce: Alerta de segurança - {alertType}. Se não foi você, acesse sua conta imediatamente.";
        await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "****";

        return $"***{phoneNumber[^4..]}";
    }
}
