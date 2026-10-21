namespace EnterpriseIam.Identity.Application.Abstractions;

/// <summary>
/// Exposes the authenticated user's identity to application use cases.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the current user's identifier, or null when unauthenticated.
    /// </summary>
    Guid? UserId
    {
        get;
    }
}
