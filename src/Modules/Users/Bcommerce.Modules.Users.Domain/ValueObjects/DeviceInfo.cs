using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.ValueObjects;

public class DeviceInfo : ValueObject
{
    public string DeviceId { get; } // Identificador único do dispositivo
    public string DeviceName { get; } // Nome legível (ex: iPhone 13 de Bruno)
    public string DeviceType { get; } // Mobile, Desktop, Tablet, etc.
    public string? OsVersion { get; } 
    public string? Browser { get; }

    public DeviceInfo(string deviceId, string deviceName, string deviceType, string? osVersion, string? browser)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("DeviceId cannot be empty.", nameof(deviceId));
        if (string.IsNullOrWhiteSpace(deviceName)) throw new ArgumentException("DeviceName cannot be empty.", nameof(deviceName));
        if (string.IsNullOrWhiteSpace(deviceType)) throw new ArgumentException("DeviceType cannot be empty.", nameof(deviceType));

        DeviceId = deviceId;
        DeviceName = deviceName;
        DeviceType = deviceType;
        OsVersion = osVersion;
        Browser = browser;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DeviceId;
        yield return DeviceName;
        yield return DeviceType;
        if (OsVersion != null) yield return OsVersion;
        if (Browser != null) yield return Browser;
    }

    public override string ToString() => $"{DeviceName} ({DeviceType})";
}
