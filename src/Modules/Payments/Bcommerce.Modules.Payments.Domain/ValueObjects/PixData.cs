using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Payments.Domain.ValueObjects;

public class PixData : ValueObject
{
    public string QrCode { get; }
    public string QrCodeUrl { get; }
    public DateTime ExpiresAt { get; }

    public PixData(string qrCode, string qrCodeUrl, DateTime expiresAt)
    {
        QrCode = qrCode;
        QrCodeUrl = qrCodeUrl;
        ExpiresAt = expiresAt;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return QrCode;
        yield return QrCodeUrl;
        yield return ExpiresAt;
    }
}
