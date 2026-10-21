namespace EnterpriseIam.Identity.Infrastructure.Provisioning;

/// <summary>Defines internal-only tenant initialization with a verified new account.</summary>
public interface ITenantBootstrapProvisioner
{
    /// <summary>Creates a tenant, a global user, and its initial membership atomically.</summary>
    /// <param name="tenantName">The new organization's display name.</param>
    /// <param name="tenantSlug">The canonical routing name.</param>
    /// <param name="userName">The globally unique initial account username.</param>
    /// <param name="email">The initial account's globally unique email.</param>
    /// <param name="password">The initial account's initial password.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The identifiers of the tenant and initial global user.</returns>
    Task<(Guid TenantId, Guid UserId)> CreateAsync(
        string tenantName,
        string tenantSlug,
        string userName,
        string email,
        string password,
        CancellationToken cancellationToken);
}
