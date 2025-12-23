using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Cart.Domain.ValueObjects;

public class ProductSnapshot : ValueObject
{
    public Guid ProductId { get; }
    public string Name { get; }
    public string Sku { get; }
    public decimal Price { get; }
    public string? ImageUrl { get; }

    public ProductSnapshot(Guid productId, string name, string sku, decimal price, string? imageUrl)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("Product SKU cannot be empty", nameof(sku));
        if (price < 0) throw new ArgumentException("Price cannot be negative", nameof(price));

        ProductId = productId;
        Name = name;
        Sku = sku;
        Price = price;
        ImageUrl = imageUrl;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return Name;
        yield return Sku;
        yield return Price;
        if (ImageUrl != null) yield return ImageUrl;
    }
}
