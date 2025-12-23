using Microsoft.Extensions.Logging;

namespace Bcommerce.Modules.Payments.Infrastructure.Webhooks;

public class WebhookProcessor : IWebhookProcessor
{
    private readonly ILogger<WebhookProcessor> _logger;

    public WebhookProcessor(ILogger<WebhookProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessWebhookAsync(string provider, string payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing webhook from {Provider}: {Payload}", provider, payload);
        // Dispatch to appropriate handler or domain service
        await Task.CompletedTask;
    }
}
