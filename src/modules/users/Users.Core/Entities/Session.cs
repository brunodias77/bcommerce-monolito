using BuildingBlocks.Domain.Entities;
using Users.Core.Events;

namespace Users.Core.Entities;

/// <summary>
/// Sessão ativa do usuário para gerenciamento de dispositivos.
/// Corresponde à tabela users.sessions no banco de dados.
/// </summary>
public class Session : Entity
{
    public Guid UserId { get; private set; }

    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string? DeviceId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? DeviceType { get; private set; }
    public string? IpAddress { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTimeOffset LastActivityAt { get; private set; }

    // Relacionamento
    public User User { get; private set; } = null!;

    private Session()
    {
    }

    public Session(
        Guid userId,
        string refreshTokenHash,
        DateTimeOffset expiresAt,
        string? deviceId = null,
        string? deviceName = null,
        string? deviceType = null,
        string? ipAddress = null,
        string? country = null,
        string? city = null,
        bool isCurrent = false)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(refreshTokenHash))
            throw new ArgumentException("Refresh token hash cannot be empty.", nameof(refreshTokenHash));

        if (expiresAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Expiration date must be in the future.", nameof(expiresAt));

        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        DeviceId = deviceId;
        DeviceName = deviceName;
        DeviceType = deviceType;
        IpAddress = ipAddress;
        Country = country;
        City = city;
        IsCurrent = isCurrent;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        LastActivityAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new SessionCreatedEvent(Id, UserId, DeviceType, IpAddress));

    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    public void SetAsCurrent()
    {
        IsCurrent = true;
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    public void UnsetCurrent()
    {
        IsCurrent = false;
    }

    public void Revoke(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revocation reason cannot be empty.", nameof(reason));

        RevokedAt = DateTimeOffset.UtcNow;
        RevokedReason = reason;

        AddDomainEvent(new SessionRevokedEvent(Id, UserId, reason));


    }

    public void UpdateLocation(string? country, string? city)
    {
        Country = country;
        City = city;
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}
