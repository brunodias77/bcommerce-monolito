using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.ValueObjects;

public class GeoLocation : ValueObject
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    public GeoLocation(decimal latitude, decimal longitude)
    {
        // Validações básicas de range
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90.", nameof(latitude));
            
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180.", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
    
    public override string ToString() => $"{Latitude}, {Longitude}";
}
