using BuildingBlocks.Domain.Entities;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Imagem de Produto.
/// Corresponde à tabela catalog.product_images no banco de dados.
/// </summary>
public class ProductImage : Entity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public string Url { get; private set; } = null!;
    public string? AltText { get; private set; }
    public string? UrlThumbnail { get; private set; }
    public string? UrlMedium { get; private set; }
    public string? UrlLarge { get; private set; }
    
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    private ProductImage() { }

    internal ProductImage(Guid productId, string url, string? altText, bool isPrimary, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        ProductId = productId;
        Url = url;
        AltText = altText;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Define esta imagem como primária.
    /// </summary>
    internal void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }

    /// <summary>
    /// Atualiza as URLs das variações de tamanho.
    /// </summary>
    public void SetVariants(string? thumbnail, string? medium, string? large)
    {
        UrlThumbnail = thumbnail;
        UrlMedium = medium;
        UrlLarge = large;
    }

    /// <summary>
    /// Atualiza o texto alternativo.
    /// </summary>
    public void UpdateAltText(string? altText)
    {
        AltText = altText;
    }

    /// <summary>
    /// Define a ordem de exibição.
    /// </summary>
    public void SetSortOrder(int order)
    {
        SortOrder = order;
    }
}
