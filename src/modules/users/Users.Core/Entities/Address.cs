using BuildingBlocks.Domain.Entities;

namespace Users.Core.Entities;

/// <summary>
/// Endereço de entrega ou cobrança do usuário.
/// Corresponde à tabela users.addresses no banco de dados.
/// </summary>
public class Address : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid UserId { get; private set; }

    // Dados do endereço
    public string? Label { get; private set; }
    public string? RecipientName { get; private set; }
    public string Street { get; private set; } = string.Empty;
    public string? Number { get; private set; }
    public string? Complement { get; private set; }
    public string? Neighborhood { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = "BR";

    // Coordenadas
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? IbgeCode { get; private set; }

    // Controle
    public bool IsDefault { get; private set; }
    public bool IsBillingAddress { get; private set; }

    // Auditoria
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    // Relacionamento
    public User User { get; private set; } = null!;

    private Address()
    {
    }

    public Address(
        Guid userId,
        string street,
        string city,
        string state,
        string postalCode,
        string? label = null,
        string? recipientName = null,
        string? number = null,
        string? complement = null,
        string? neighborhood = null,
        bool isDefault = false,
        bool isBillingAddress = false)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.", nameof(state));

        if (!IsValidStateFormat(state))
            throw new ArgumentException("State must be a 2-letter code (e.g., SP, RJ).", nameof(state));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty.", nameof(postalCode));

        if (!IsValidPostalCodeFormat(postalCode))
            throw new ArgumentException("Invalid postal code format. Expected: 00000-000", nameof(postalCode));

        UserId = userId;
        Label = label;
        RecipientName = recipientName;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state.ToUpperInvariant();
        PostalCode = postalCode;
        IsDefault = isDefault;
        IsBillingAddress = isBillingAddress;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string street,
        string city,
        string state,
        string postalCode,
        string? label = null,
        string? recipientName = null,
        string? number = null,
        string? complement = null,
        string? neighborhood = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.", nameof(state));

        if (!IsValidStateFormat(state))
            throw new ArgumentException("State must be a 2-letter code (e.g., SP, RJ).", nameof(state));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty.", nameof(postalCode));

        if (!IsValidPostalCodeFormat(postalCode))
            throw new ArgumentException("Invalid postal code format. Expected: 00000-000", nameof(postalCode));

        Label = label;
        RecipientName = recipientName;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state.ToUpperInvariant();
        PostalCode = postalCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsBillingAddress()
    {
        IsBillingAddress = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetBillingAddress()
    {
        IsBillingAddress = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCoordinates(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateIbgeCode(string ibgeCode)
    {
        IbgeCode = ibgeCode;
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

    private static bool IsValidPostalCodeFormat(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        // Formato: 00000-000 ou 00000000
        var pattern = @"^\d{5}-?\d{3}$";
        return System.Text.RegularExpressions.Regex.IsMatch(postalCode, pattern);
    }

    private static bool IsValidStateFormat(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            return false;

        // Formato: XX (duas letras maiúsculas)
        var pattern = @"^[A-Z]{2}$";
        return System.Text.RegularExpressions.Regex.IsMatch(state.ToUpperInvariant(), pattern);
    }
}
