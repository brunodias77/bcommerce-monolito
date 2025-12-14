using BuildingBlocks.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Users.Core.Entities;

/// <summary>
/// Entidade de usuário baseada no ASP.NET Identity.
/// Corresponde à tabela users.asp_net_users no banco de dados.
/// </summary>
public class User : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Relacionamentos
    public Profile? Profile { get; private set; }
    public ICollection<Address> Addresses { get; private set; } = new List<Address>();
    public ICollection<Session> Sessions { get; private set; } = new List<Session>();
    public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
    public ICollection<LoginHistory> LoginHistories { get; private set; } = new List<LoginHistory>();
    public NotificationPreferences? NotificationPreferences { get; private set; }

    private User()
    {
    }

    public User Create(string email, string userName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be empty.", nameof(userName));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = userName.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0
        };
        
        // ⭐ Levanta Domain Event
        // user.AddDomainEvent(new UserCreatedEvent(user.Id, email, userName));

        return user;
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        EmailConfirmed = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePhoneNumber(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
        PhoneNumberConfirmed = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmPhoneNumber()
    {
        PhoneNumberConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LockUser(DateTimeOffset lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnlockUser()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CreateProfile(
        string firstName,
        string lastName,
        DateTime? birthDate = null,
        string? cpf = null)
    {
        Profile = new Profile(Id, firstName, lastName, birthDate, cpf);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAddress(
        string street,
        string city,
        string state,
        string postalCode,
        string? label = null,
        string? recipientName = null,
        string? number = null,
        string? complement = null,
        string? neighborhood = null,
        bool isDefault = false)
    {
        var address = new Address(
            Id,
            street,
            city,
            state,
            postalCode,
            label,
            recipientName,
            number,
            complement,
            neighborhood,
            isDefault);

        if (isDefault)
        {
            foreach (var existingAddress in Addresses.Where(a => !a.IsDeleted))
            {
                existingAddress.UnsetDefault();
            }
        }

        Addresses.Add(address);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CreateSession(
        string refreshTokenHash,
        DateTimeOffset expiresAt,
        string? deviceId = null,
        string? deviceName = null,
        string? deviceType = null,
        string? ipAddress = null)
    {
        var session = new Session(
            Id,
            refreshTokenHash,
            expiresAt,
            deviceId,
            deviceName,
            deviceType,
            ipAddress);

        Sessions.Add(session);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNotification(
        string title,
        string message,
        string notificationType,
        string? referenceType = null,
        Guid? referenceId = null,
        string? actionUrl = null)
    {
        var notification = new Notification(
            Id,
            title,
            message,
            notificationType,
            referenceType,
            referenceId,
            actionUrl);

        Notifications.Add(notification);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CreateNotificationPreferences()
    {
        NotificationPreferences = new NotificationPreferences(Id);
        UpdatedAt = DateTime.UtcNow;
    }
}
