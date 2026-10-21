namespace EnterpriseIam.Identity.Domain.Tenants;

/// <summary>
/// Describes whether an organization can authorize tenant-scoped operations.
/// </summary>
public enum TenantStatus : byte
{
    /// <summary>The organization can accept authorized requests.</summary>
    Active = 1,

    /// <summary>The organization cannot authorize tenant-scoped requests.</summary>
    Suspended = 2
}
