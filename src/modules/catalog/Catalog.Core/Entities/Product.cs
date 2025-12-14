using BuildingBlocks.Domain.Entities;
using Catalog.Core.Enums;
using Catalog.Core.Events;
using Catalog.Core.Exceptions;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Produto - Aggregate Root do módulo Catalog.
/// Corresponde à tabela catalog.products no banco de dados.
/// </summary>
public class Product : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    // Identificação
    public string Sku { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Barcode { get; private set; }
    
    // Dados do produto
    public string Name { get; private set; } = null!;
    public string? ShortDescription { get; private set; }
    public string? Description { get; private set; }
    
    // Preços
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public decimal? CostPrice { get; private set; }
    
    // Estoque
    public int Stock { get; private set; }
    public int ReservedStock { get; private set; }
    public int LowStockThreshold { get; private set; }
    
    // Dimensões
    public int? WeightGrams { get; private set; }
    public decimal? HeightCm { get; private set; }
    public decimal? WidthCm { get; private set; }
    public decimal? LengthCm { get; private set; }
    
    // SEO
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    
    // Status e flags
    public ProductStatus Status { get; private set; }
    public bool IsFeatured { get; private set; }
    public bool IsDigital { get; private set; }
    public bool RequiresShipping { get; private set; }
    
    // Flexível
    public string? Attributes { get; private set; } // JSON
    public string[]? Tags { get; private set; }
    
    // Timestamps
    public DateTime? PublishedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    // Relacionamentos
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    
    public Guid? BrandId { get; private set; }
    public Brand? Brand { get; private set; }
    
    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    
    private readonly List<StockMovement> _stockMovements = new();
    public IReadOnlyCollection<StockMovement> StockMovements => _stockMovements.AsReadOnly();
    
    private readonly List<StockReservation> _stockReservations = new();
    public IReadOnlyCollection<StockReservation> StockReservations => _stockReservations.AsReadOnly();
    
    private readonly List<ProductReview> _reviews = new();
    public IReadOnlyCollection<ProductReview> Reviews => _reviews.AsReadOnly();

    private Product() { }

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    public static Product Create(
        string sku,
        string name,
        decimal price,
        int initialStock = 0,
        Guid? categoryId = null,
        Guid? brandId = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty.", nameof(sku));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        
        if (price < 0)
            throw new InvalidPriceException("Price cannot be negative.");
        
        if (initialStock < 0)
            throw new ArgumentException("Initial stock cannot be negative.", nameof(initialStock));

        var product = new Product
        {
            Sku = sku,
            Name = name,
            Slug = GenerateSlug(name),
            Price = price,
            Stock = initialStock,
            ReservedStock = 0,
            LowStockThreshold = 10,
            Status = ProductStatus.Draft,
            IsFeatured = false,
            IsDigital = false,
            RequiresShipping = true,
            CategoryId = categoryId,
            BrandId = brandId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, sku, name, price));

        return product;
    }

    /// <summary>
    /// Atualiza as informações básicas do produto.
    /// </summary>
    public void UpdateInfo(
        string name,
        string? shortDescription,
        string? description,
        string? metaTitle,
        string? metaDescription)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        Slug = GenerateSlug(name);
        ShortDescription = shortDescription;
        Description = description;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza o preço do produto.
    /// </summary>
    public void UpdatePrice(decimal newPrice, decimal? compareAtPrice = null, decimal? costPrice = null)
    {
        if (newPrice < 0)
            throw new InvalidPriceException("Price cannot be negative.");
        
        if (compareAtPrice.HasValue && compareAtPrice <= newPrice)
            throw new InvalidPriceException("Compare at price must be greater than the regular price.");

        var oldPrice = Price;
        Price = newPrice;
        CompareAtPrice = compareAtPrice;
        CostPrice = costPrice;
        UpdatedAt = DateTime.UtcNow;

        if (oldPrice != newPrice)
        {
            AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
        }
    }

    /// <summary>
    /// Publica o produto (torna visível para clientes).
    /// </summary>
    public void Publish()
    {
        if (Status == ProductStatus.Discontinued)
            throw new InvalidOperationException("Cannot publish a discontinued product.");

        Status = ProductStatus.Active;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductPublishedEvent(Id, Name));
    }

    /// <summary>
    /// Desativa o produto.
    /// </summary>
    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Descontinua o produto permanentemente.
    /// </summary>
    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reserva estoque para um pedido.
    /// </summary>
    public StockReservation ReserveStock(int quantity, string referenceType, Guid referenceId, TimeSpan reservationDuration)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        var availableStock = Stock - ReservedStock;
        if (quantity > availableStock)
            throw new InsufficientStockException(Id, availableStock, quantity);

        ReservedStock += quantity;
        UpdatedAt = DateTime.UtcNow;

        var reservation = new StockReservation(
            Id,
            referenceType,
            referenceId,
            quantity,
            DateTime.UtcNow.Add(reservationDuration));

        _stockReservations.Add(reservation);

        // Registrar movimento de estoque
        var movement = new StockMovement(
            Id,
            Enums.StockMovementType.Reserve,
            quantity,
            referenceType,
            referenceId,
            Stock,
            Stock);

        _stockMovements.Add(movement);

        AddDomainEvent(new StockReservedEvent(Id, quantity, referenceType, referenceId));

        // Verificar se ficou sem estoque
        if (Stock - ReservedStock <= 0 && Status == ProductStatus.Active)
        {
            Status = ProductStatus.OutOfStock;
        }

        return reservation;
    }

    /// <summary>
    /// Libera estoque reservado.
    /// </summary>
    public void ReleaseStock(Guid reservationId)
    {
        var reservation = _stockReservations.FirstOrDefault(r => r.Id == reservationId && r.ReleasedAt == null);
        if (reservation == null)
            throw new InvalidOperationException($"Reservation {reservationId} not found or already released.");

        reservation.Release();
        ReservedStock -= reservation.Quantity;
        UpdatedAt = DateTime.UtcNow;

        // Registrar movimento de estoque
        var movement = new StockMovement(
            Id,
            Enums.StockMovementType.Release,
            reservation.Quantity,
            reservation.ReferenceType,
            reservation.ReferenceId,
            Stock,
            Stock);

        _stockMovements.Add(movement);

        AddDomainEvent(new StockReleasedEvent(Id, reservation.Quantity, reservation.ReferenceType, reservation.ReferenceId));

        // Verificar se voltou a ter estoque
        if (Stock - ReservedStock > 0 && Status == ProductStatus.OutOfStock)
        {
            Status = ProductStatus.Active;
        }
    }

    /// <summary>
    /// Ajusta o estoque (entrada ou saída).
    /// </summary>
    public void AdjustStock(int quantity, string reason, Guid? performedBy = null)
    {
        var stockBefore = Stock;
        Stock += quantity;
        
        if (Stock < 0)
            Stock = 0;

        UpdatedAt = DateTime.UtcNow;

        var movementType = quantity >= 0 ? Enums.StockMovementType.In : Enums.StockMovementType.Adjustment;
        
        var movement = new StockMovement(
            Id,
            movementType,
            Math.Abs(quantity),
            null,
            null,
            stockBefore,
            Stock,
            reason,
            performedBy);

        _stockMovements.Add(movement);

        // Atualizar status baseado no estoque
        var availableStock = Stock - ReservedStock;
        if (availableStock <= 0 && Status == ProductStatus.Active)
        {
            Status = ProductStatus.OutOfStock;
        }
        else if (availableStock > 0 && Status == ProductStatus.OutOfStock)
        {
            Status = ProductStatus.Active;
        }
    }

    /// <summary>
    /// Adiciona uma imagem ao produto.
    /// </summary>
    public ProductImage AddImage(string url, string? altText = null, bool isPrimary = false)
    {
        if (isPrimary)
        {
            foreach (var img in _images)
            {
                img.SetPrimary(false);
            }
        }

        var image = new ProductImage(Id, url, altText, isPrimary, _images.Count);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;

        return image;
    }

    /// <summary>
    /// Remove uma imagem do produto.
    /// </summary>
    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            _images.Remove(image);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Retorna o estoque disponível (total - reservado).
    /// </summary>
    public int AvailableStock => Stock - ReservedStock;

    /// <summary>
    /// Indica se o estoque está baixo.
    /// </summary>
    public bool IsLowStock => AvailableStock <= LowStockThreshold;

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
