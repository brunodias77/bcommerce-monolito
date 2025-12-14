using BuildingBlocks.Domain.Entities;
using Catalog.Core.Events;

namespace Catalog.Core.Entities;

/// <summary>
/// Entidade de Avaliação de Produto.
/// Corresponde à tabela catalog.product_reviews no banco de dados.
/// </summary>
public class ProductReview : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    
    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string? Comment { get; private set; }
    
    public bool IsVerifiedPurchase { get; private set; }
    public bool IsApproved { get; private set; }
    
    public string? SellerResponse { get; private set; }
    public DateTime? SellerRespondedAt { get; private set; }
    
    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private ProductReview() { }

    /// <summary>
    /// Cria uma nova avaliação de produto.
    /// </summary>
    public static ProductReview Create(
        Guid productId,
        Guid userId,
        int rating,
        string? title,
        string? comment,
        bool isVerifiedPurchase = false,
        Guid? orderId = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        var review = new ProductReview
        {
            ProductId = productId,
            UserId = userId,
            OrderId = orderId,
            Rating = rating,
            Title = title,
            Comment = comment,
            IsVerifiedPurchase = isVerifiedPurchase,
            IsApproved = false, // Requer moderação
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        review.AddDomainEvent(new ReviewCreatedEvent(review.Id, productId, userId, rating));

        return review;
    }

    /// <summary>
    /// Aprova a avaliação para exibição.
    /// </summary>
    public void Approve()
    {
        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejeita/reprova a avaliação.
    /// </summary>
    public void Reject()
    {
        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adiciona resposta do vendedor.
    /// </summary>
    public void AddSellerResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Response cannot be empty.", nameof(response));

        SellerResponse = response;
        SellerRespondedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza a avaliação.
    /// </summary>
    public void Update(int rating, string? title, string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        Rating = rating;
        Title = title;
        Comment = comment;
        IsApproved = false; // Precisa ser reaprovado após edição
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
}
