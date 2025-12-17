using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;

namespace Bcommerce.Modules.Catalog.Domain.Entities;

public class Brand : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; }

    // Required for EF
    protected Brand() { }

    public Brand(string name, Slug slug)
    {
        Id = Guid.NewGuid();
        Name = name;
        Slug = slug;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? logoUrl)
    {
        Name = name;
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
