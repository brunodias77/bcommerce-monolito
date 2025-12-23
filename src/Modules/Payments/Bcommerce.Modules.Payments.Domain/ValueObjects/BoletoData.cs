using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Payments.Domain.ValueObjects;

public class BoletoData : ValueObject
{
    public string BarCode { get; }
    public string DigitableLine { get; }
    public string Url { get; }
    public DateTime DueDate { get; }

    public BoletoData(string barCode, string digitableLine, string url, DateTime dueDate)
    {
        BarCode = barCode;
        DigitableLine = digitableLine;
        Url = url;
        DueDate = dueDate;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BarCode;
        yield return DigitableLine;
        yield return Url;
        yield return DueDate;
    }
}
