using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Catalog.Domain.Enums;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;

namespace Bcommerce.Modules.Catalog.Domain.Entities;

public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Sku Sku { get; private set; }
    public Slug Slug { get; private set; }
    public Money Price { get; private set; }
    public Stock Stock { get; private set; }
    public ProductDimensions? Dimensions { get; private set; }
    public ProductStatus Status { get; private set; }
    
    public Guid CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }

    // Navigation properties
    public virtual Category Category { get; private set; } = null!;
    public virtual Brand? Brand { get; private set; }
    
    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    
    private readonly List<ProductReview> _reviews = new();
    public IReadOnlyCollection<ProductReview> Reviews => _reviews.AsReadOnly();

    // EF Core
    protected Product() { }

    public Product(string name, Sku sku, Slug slug, Money price, Guid categoryId, Guid? brandId = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Sku = sku;
        Slug = slug;
        Price = price;
        CategoryId = categoryId;
        BrandId = brandId;
        Status = ProductStatus.Draft;
        Stock = new Stock(0);
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateInformation(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddImage(ProductImage image)
    {
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        Stock = Stock.Add(quantity);
        UpdatedAt = DateTime.UtcNow;
    }
}
