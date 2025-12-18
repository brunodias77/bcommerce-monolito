using Bcommerce.Modules.Orders.Domain.Entities;

namespace Bcommerce.Modules.Orders.Infrastructure.Services;

public class InvoiceService
{
    public async Task<string> GenerateInvoiceAsync(Order order)
    {
        // Placeholder implementation
        await Task.Delay(100);
        return $"https://invoices.bcommerce.com/{order.Id}.pdf";
    }
}
