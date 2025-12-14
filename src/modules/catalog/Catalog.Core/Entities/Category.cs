using BuildingBlocks.Domain.Entities;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Categoria (hierárquica).
/// Corresponde à tabela catalog.categories no banco de dados.
/// </summary>
public class Category : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public Guid? ParentId { get; private set; }
    public Category? Parent { get; private set; }
    
    public string? Path { get; private set; }
    public int Depth { get; private set; }
    
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    
    // SEO
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    
    // Controle
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    
    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    // Relacionamentos
    private readonly List<Category> _children = new();
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();
    
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { }

    /// <summary>
    /// Cria uma nova categoria.
    /// </summary>
    public static Category Create(
        string name,
        string? description = null,
        Category? parent = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var category = new Category
        {
            Name = name,
            Slug = GenerateSlug(name),
            Description = description,
            IsActive = true,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (parent != null)
        {
            category.SetParent(parent);
        }
        else
        {
            category.ParentId = null;
            category.Depth = 0;
            category.Path = category.Id.ToString();
        }

        return category;
    }

    /// <summary>
    /// Define a categoria pai.
    /// </summary>
    public void SetParent(Category parent)
    {
        if (parent.Id == Id)
            throw new InvalidOperationException("A category cannot be its own parent.");
        
        if (parent.Depth >= 5)
            throw new InvalidOperationException("Maximum category depth (5) exceeded.");

        ParentId = parent.Id;
        Parent = parent;
        Depth = parent.Depth + 1;
        Path = $"{parent.Path}/{Id}";
        UpdatedAt = DateTime.UtcNow;
        
        parent._children.Add(this);
    }

    /// <summary>
    /// Atualiza as informações da categoria.
    /// </summary>
    public void Update(
        string name,
        string? description,
        string? imageUrl,
        string? metaTitle,
        string? metaDescription)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Slug = GenerateSlug(name);
        Description = description;
        ImageUrl = imageUrl;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ativa a categoria.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desativa a categoria.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Define a ordem de exibição.
    /// </summary>
    public void SetSortOrder(int order)
    {
        SortOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    // ISoftDeletable
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a")
            .Replace("é", "e").Replace("ê", "e")
            .Replace("í", "i")
            .Replace("ó", "o").Replace("õ", "o").Replace("ô", "o")
            .Replace("ú", "u")
            .Replace("ç", "c");
    }
}
