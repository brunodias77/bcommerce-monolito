namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

public interface IEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
