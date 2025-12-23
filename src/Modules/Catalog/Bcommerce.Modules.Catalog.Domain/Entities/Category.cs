using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;

namespace Bcommerce.Modules.Catalog.Domain.Entities;

public class Category : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public int Depth { get; private set; }
    public bool IsActive { get; private set; }
    
    // Self-reference for hierarchy
    public Category? Parent { get; private set; }
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();
    private readonly List<Category> _subCategories = new();

    // Required for EF
    protected Category() { }

    public Category(string name, Slug slug, int depth = 0, Guid? parentId = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Slug = slug;
        Depth = depth;
        ParentId = parentId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
