using Bcommerce.BuildingBlocks.Application.Abstractions.Services;

namespace Bcommerce.BuildingBlocks.Infrastructure.Time;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
