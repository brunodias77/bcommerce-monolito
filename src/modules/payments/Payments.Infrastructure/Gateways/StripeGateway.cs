namespace Payments.Infrastructure.Gateways;

// Placeholder implementation
public class StripeGateway
{
    public Task<string> AuthorizeAsync(decimal amount, string currency, string token)
    {
        return Task.FromResult($"stripe_ch_{Guid.NewGuid()}");
    }
    
    public Task<string> CaptureAsync(string authorizationId)
    {
        return Task.FromResult($"stripe_tr_{Guid.NewGuid()}");
    }
}
