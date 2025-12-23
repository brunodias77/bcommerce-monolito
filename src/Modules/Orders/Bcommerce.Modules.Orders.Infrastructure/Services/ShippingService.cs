namespace Bcommerce.Modules.Orders.Infrastructure.Services;

public class ShippingService
{
    public async Task<decimal> CalculateShippingAsync(Guid addressId, IEnumerable<Guid> productIds)
    {
        // Placeholder implementation
        await Task.Delay(100);
        return 15.00m; // Flat rate for now
    }
}
