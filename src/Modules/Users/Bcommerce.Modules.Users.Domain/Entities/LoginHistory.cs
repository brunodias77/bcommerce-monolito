using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Users.Domain.ValueObjects;

namespace Bcommerce.Modules.Users.Domain.Entities;

public class LoginHistory : Entity<Guid>
{
    public Guid UserId { get; private set; }
    
    public string LoginProvider { get; private set; } // Local, Google, Facebook
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    
    public DeviceInfo? DeviceInfo { get; private set; }
    
    public bool Success { get; private set; }
    public string? FailureReason { get; private set; }

    protected LoginHistory() { }

    public LoginHistory(
        Guid userId,
        string loginProvider,
        string? ipAddress,
        string? userAgent,
        string? country,
        string? city,
        DeviceInfo? deviceInfo,
        bool success,
        string? failureReason) : base(Guid.NewGuid())
    {
        UserId = userId;
        LoginProvider = loginProvider;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Country = country;
        City = city;
        DeviceInfo = deviceInfo;
        Success = success;
        FailureReason = failureReason;
    }
}
