namespace Bcommerce.BuildingBlocks.Caching.Abstractions;

public interface ICacheKeyGenerator
{
    string GenerateKey(params object[] parts);
}
