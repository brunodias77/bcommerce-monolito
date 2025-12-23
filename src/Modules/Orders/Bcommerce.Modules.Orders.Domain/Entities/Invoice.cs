using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Orders.Domain.Entities;

public class Invoice : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public string Url { get; private set; }
    public DateTime IssuedAt { get; private set; }

    private Invoice() { }

    public Invoice(Guid orderId, string invoiceNumber, string url)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        InvoiceNumber = invoiceNumber;
        Url = url;
        IssuedAt = DateTime.UtcNow;
    }
}
