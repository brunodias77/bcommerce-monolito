namespace Bcommerce.Modules.Payments.Infrastructure.Webhooks;

public interface IWebhookProcessor
{
    Task ProcessWebhookAsync(string provider, string payload, CancellationToken cancellationToken = default);
}
