using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.ValueObjects;

public class ProductDimensions : ValueObject
{
    public decimal Height { get; private set; }
    public decimal Width { get; private set; }
    public decimal Depth { get; private set; }
    public decimal Weight { get; private set; }

    public ProductDimensions(decimal height, decimal width, decimal depth, decimal weight)
    {
        if (height <= 0) throw new ArgumentException("Height must be greater than zero", nameof(height));
        if (width <= 0) throw new ArgumentException("Width must be greater than zero", nameof(width));
        if (depth <= 0) throw new ArgumentException("Depth must be greater than zero", nameof(depth));
        if (weight <= 0) throw new ArgumentException("Weight must be greater than zero", nameof(weight));

        Height = height;
        Width = width;
        Depth = depth;
        Weight = weight;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Height;
        yield return Width;
        yield return Depth;
        yield return Weight;
    }
}
