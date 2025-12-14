using System.ComponentModel.DataAnnotations;

namespace Users.Presentation.Requests;

/// <summary>
/// Request para adicionar endereço.
/// </summary>
public record AddAddressRequest(
    [MaxLength(50)]
    string? Label,

    [MaxLength(100)]
    string? RecipientName,

    [Required]
    [MaxLength(255)]
    string Street,

    [MaxLength(20)]
    string? Number,

    [MaxLength(100)]
    string? Complement,

    [MaxLength(100)]
    string? Neighborhood,

    [Required]
    [MaxLength(100)]
    string City,

    [Required]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Estado deve ter 2 letras maiúsculas")]
    string State,

    [Required]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "CEP deve conter 8 dígitos")]
    string PostalCode,

    bool IsDefault = false
);
