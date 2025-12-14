using BuildingBlocks.Domain.Exceptions;

namespace Users.Core.Exceptions;

/// <summary>
/// Exception thrown when a user profile is not found.
/// </summary>
public sealed class ProfileNotFoundException : EntityNotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileNotFoundException"/> class.
    /// </summary>
    /// <param name="profileId">The ID of the profile that was not found</param>
    public ProfileNotFoundException(Guid profileId)
        : base($"Profile with ID '{profileId}' was not found.")
    {
        ProfileId = profileId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileNotFoundException"/> class.
    /// </summary>
    /// <param name="userId">The user ID whose profile was not found</param>
    /// <param name="isUserIdSearch">Indicates this is a search by user ID</param>
    public ProfileNotFoundException(Guid userId, bool isUserIdSearch)
        : base($"Profile for user ID '{userId}' was not found.")
    {
        UserId = userId;
    }

    /// <summary>
    /// Gets the profile ID that was not found.
    /// </summary>
    public Guid? ProfileId { get; }

    /// <summary>
    /// Gets the user ID whose profile was not found.
    /// </summary>
    public Guid? UserId { get; }
}
