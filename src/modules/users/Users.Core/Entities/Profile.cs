using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Models;

namespace Users.Core.Entities;

/// <summary>
/// Perfil estendido do usuário.
/// Corresponde à tabela users.profiles no banco de dados.
/// </summary>
public class Profile : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public Guid UserId { get; private set; }

    // Dados pessoais
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? Gender { get; private set; }
    public Cpf? Cpf { get; private set; }

    // Preferências
    public string PreferredLanguage { get; private set; } = "pt-BR";
    public string PreferredCurrency { get; private set; } = "BRL";
    public bool NewsletterSubscribed { get; private set; }

    // Termos
    public DateTime? AcceptedTermsAt { get; private set; }
    public DateTime? AcceptedPrivacyAt { get; private set; }

    // Auditoria
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    // Relacionamento
    public User User { get; private set; } = null!;

    private Profile()
    {
    }

    public Profile(
        Guid userId,
        string firstName,
        string lastName,
        DateTime? birthDate = null,
        string? cpf = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = $"{firstName} {lastName}";
        BirthDate = birthDate;
        Cpf = cpf != null ? BuildingBlocks.Domain.Models.Cpf.Create(cpf) : null;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(
        string firstName,
        string lastName,
        DateTime? birthDate = null,
        string? gender = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        DisplayName = $"{firstName} {lastName}";
        BirthDate = birthDate;
        Gender = gender;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        DisplayName = displayName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCpf(string cpf)
    {
        Cpf = BuildingBlocks.Domain.Models.Cpf.Create(cpf);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePreferredLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be empty.", nameof(language));

        PreferredLanguage = language;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePreferredCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        PreferredCurrency = currency;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SubscribeToNewsletter()
    {
        NewsletterSubscribed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsubscribeFromNewsletter()
    {
        NewsletterSubscribed = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptTerms()
    {
        AcceptedTermsAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptPrivacyPolicy()
    {
        AcceptedPrivacyAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
