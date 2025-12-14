using System.ComponentModel.DataAnnotations;

namespace Users.Presentation.Requests;

/// <summary>
/// Request para atualização de perfil.
/// </summary>
public record UpdateProfileRequest(
    [MaxLength(100)]
    string? FirstName,

    [MaxLength(100)]
    string? LastName,

    DateTime? BirthDate,

    [MaxLength(20)]
    string? Gender,

    [MaxLength(500)]
    string? AvatarUrl,

    [MaxLength(500)]
    string? Bio
);
