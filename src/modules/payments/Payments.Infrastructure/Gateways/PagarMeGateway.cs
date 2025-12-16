namespace Payments.Infrastructure.Gateways;

// Placeholder implementation
public class PagarMeGateway
{
    public Task<string> GeneratePixAsync(decimal amount)
    {
        return Task.FromResult($"pagarme_pix_{Guid.NewGuid()}");
    }
    
    public Task<string> GenerateBoletoAsync(decimal amount)
    {
        return Task.FromResult($"pagarme_bol_{Guid.NewGuid()}");
    }
}
