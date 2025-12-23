using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Catalog.Domain.Entities;

public class ProductImage : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    protected ProductImage() { }

    public ProductImage(Guid productId, string url, bool isPrimary = false, int sortOrder = 0)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Url = url;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
    }
}
