namespace Bcommerce.BuildingBlocks.Application.Abstractions.Services;

/// <summary>
/// Contrato para serviço de envio de emails transacionais.
/// </summary>
/// <remarks>
/// Abstração para provedores de email (SendGrid, AWS SES, SMTP).
/// - Envia notificações assíncronas
/// - Suporta templates HTML ou texto plano (dependendo da implementação)
/// - Deve tratar falhas de envio ou colocar em fila (Outbox)
/// 
/// Exemplo de uso:
/// <code>
/// public async Task Handle(PedidoCriadoEvent evento, CancellationToken ct)
/// {
///     await _emailService.SendEmailAsync(
///         evento.EmailCliente, 
///         "Pedido Confirmado", 
///         $"Seu pedido {evento.Id} foi recebido!", 
///         ct);
/// }
/// </code>
/// </remarks>
public interface IEmailService
{
    /// <summary>
    /// Envia um email simples de texto/HTML.
    /// </summary>
    /// <param name="to">Endereço de email do destinatário.</param>
    /// <param name="subject">Assunto do email.</param>
    /// <param name="body">Conteúdo do email (pode ser HTML).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
