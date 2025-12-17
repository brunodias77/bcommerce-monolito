using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Users.Domain.ValueObjects;

namespace Bcommerce.Modules.Users.Domain.Entities;

public class Session : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    
    public DeviceInfo? DeviceInfo { get; private set; }
    
    public string? IpAddress { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    
    public bool IsCurrent { get; private set; } // Indica se é a sessão atual da requisição (contextual)
    
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }
    public DateTime LastActivityAt { get; private set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

    protected Session() { }

    public Session(
        Guid userId,
        string refreshTokenHash,
        DateTime expiresAt,
        DeviceInfo? deviceInfo,
        string? ipAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        ExpiresAt = expiresAt;
        DeviceInfo = deviceInfo;
        IpAddress = ipAddress;
        LastActivityAt = DateTime.UtcNow;
    }

    public void Revoke(string reason)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        // Evento de sessão revogada
    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }
}
