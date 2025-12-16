namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public void UndoDelete();
}
