using Bcommerce.BuildingBlocks.Domain.Base;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bcommerce.BuildingBlocks.Infrastructure.Data.Converters;

public class EnumerationConverter<T> : ValueConverter<T, int>
    where T : Enumeration
{
    public EnumerationConverter() : base(
        v => v.Id,
        v => CreateState(v))
    {
    }

    private static T CreateState(int id)
    {
        return Enumeration.GetAll<T>().Single(x => x.Id == id);
    }
}
