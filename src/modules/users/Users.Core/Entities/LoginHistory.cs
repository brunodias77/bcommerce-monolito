using BuildingBlocks.Domain.Entities;
using System.Text.Json;

namespace Users.Core.Entities;

/// <summary>
/// Histórico de login do usuário.
/// Corresponde à tabela users.login_history no banco de dados.
/// </summary>
public class LoginHistory : Entity
{
    public Guid UserId { get; private set; }

    public string LoginProvider { get; private set; } = "Local";
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public string? DeviceType { get; private set; }
    public string? DeviceInfo { get; private set; } // JSON serializado
    public bool Success { get; private set; }
    public string? FailureReason { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Relacionamento
    public User User { get; private set; } = null!;

    private LoginHistory()
    {
    }

    public LoginHistory(
        Guid userId,
        bool success,
        string? loginProvider = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? country = null,
        string? city = null,
        string? deviceType = null,
        object? deviceInfo = null,
        string? failureReason = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (!success && string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason is required when success is false.", nameof(failureReason));

        UserId = userId;
        LoginProvider = loginProvider ?? "Local";
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Country = country;
        City = city;
        DeviceType = deviceType;

        if (deviceInfo != null)
        {
            DeviceInfo = JsonSerializer.Serialize(deviceInfo);
        }

        Success = success;
        FailureReason = failureReason;
        CreatedAt = DateTime.UtcNow;
    }

    public static LoginHistory CreateSuccessful(
        Guid userId,
        string? loginProvider = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? country = null,
        string? city = null,
        string? deviceType = null,
        object? deviceInfo = null)
    {
        return new LoginHistory(
            userId,
            true,
            loginProvider,
            ipAddress,
            userAgent,
            country,
            city,
            deviceType,
            deviceInfo);
    }

    public static LoginHistory CreateFailed(
        Guid userId,
        string failureReason,
        string? loginProvider = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? country = null,
        string? city = null,
        string? deviceType = null,
        object? deviceInfo = null)
    {
        return new LoginHistory(
            userId,
            false,
            loginProvider,
            ipAddress,
            userAgent,
            country,
            city,
            deviceType,
            deviceInfo,
            failureReason);
    }

    public T? GetDeviceInfo<T>() where T : class
    {
        if (string.IsNullOrWhiteSpace(DeviceInfo))
            return null;

        return JsonSerializer.Deserialize<T>(DeviceInfo);
    }
}
