using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Catalog.Domain.ValueObjects;

namespace Bcommerce.Modules.Catalog.Domain.Entities;

public class ProductReview : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public Rating Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsApproved { get; private set; }

    protected ProductReview() { }

    public ProductReview(Guid productId, Guid userId, Rating rating, string? comment)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
        IsApproved = false; // Requires moderation
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
