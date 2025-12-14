using BuildingBlocks.Domain.Entities;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Marca (Brand).
/// Corresponde à tabela catalog.brands no banco de dados.
/// </summary>
public class Brand : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    
    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    // Relacionamentos
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Brand() { }

    /// <summary>
    /// Cria uma nova marca.
    /// </summary>
    public static Brand Create(string name, string? description = null, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new Brand
        {
            Name = name,
            Slug = GenerateSlug(name),
            Description = description,
            LogoUrl = logoUrl,
            IsActive = true,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Atualiza as informações da marca.
    /// </summary>
    public void Update(string name, string? description, string? logoUrl, string? websiteUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Slug = GenerateSlug(name);
        Description = description;
        LogoUrl = logoUrl;
        WebsiteUrl = websiteUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

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
