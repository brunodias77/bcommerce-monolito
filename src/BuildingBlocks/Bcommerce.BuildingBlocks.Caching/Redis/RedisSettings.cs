namespace Bcommerce.BuildingBlocks.Caching.Redis;

public class RedisSettings
{
    public const string SectionName = "RedisSettings";

    public string ConnectionString { get; set; } = "localhost";
    public int InstanceNumber { get; set; } = -1;
}
