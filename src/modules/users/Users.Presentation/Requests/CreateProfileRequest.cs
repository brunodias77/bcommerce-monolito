using System.ComponentModel.DataAnnotations;

namespace Users.Presentation.Requests;

/// <summary>
/// Request para criação de perfil.
/// </summary>
public record CreateProfileRequest(
    [Required]
    [MaxLength(100)]
    string FirstName,

    [Required]
    [MaxLength(100)]
    string LastName,

    DateTime? BirthDate,

    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
    string? Cpf,

    [MaxLength(20)]
    string? Gender
);
