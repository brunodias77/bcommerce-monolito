using BuildingBlocks.Domain.Events;

namespace Catalog.Core.Events;

/// <summary>
/// Evento levantado quando uma nova avaliação é criada.
/// </summary>
[AggregateType("ProductReview")]
public class ReviewCreatedEvent : DomainEvent
{
    public Guid ReviewId { get; }
    public Guid ProductId { get; }
    public Guid UserId { get; }
    public int Rating { get; }

    public override Guid AggregateId => ReviewId;

    public ReviewCreatedEvent(Guid reviewId, Guid productId, Guid userId, int rating)
    {
        ReviewId = reviewId;
        ProductId = productId;
        UserId = userId;
        Rating = rating;
    }
}
