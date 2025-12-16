using BuildingBlocks.Domain.Entities;

namespace Orders.Core.Entities;

public class Invoice : Entity
{
    public Guid OrderId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public string? InvoiceKey { get; private set; }
    public string? InvoiceSeries { get; private set; }
    public string? PdfUrl { get; private set; }
    public string? XmlUrl { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Invoice() { }

    public Invoice(Guid orderId, string invoiceNumber, string? invoiceKey, string? invoiceSeries, string? pdfUrl, string? xmlUrl, DateTime issuedAt)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        InvoiceNumber = invoiceNumber;
        InvoiceKey = invoiceKey;
        InvoiceSeries = invoiceSeries;
        PdfUrl = pdfUrl;
        XmlUrl = xmlUrl;
        IssuedAt = issuedAt;
        CreatedAt = DateTime.UtcNow;
    }
}
