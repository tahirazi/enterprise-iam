namespace EnterpriseIam.Identity.Domain.Tenants;

/// <summary>Represents a globally identified user's membership in one organization.</summary>
public sealed class TenantMembership
{
    private TenantMembership() { }

    private TenantMembership(Guid tenantId, Guid userId, DateTime utcNow)
    {
        TenantId = tenantId;
        UserId = userId;
        Status = MembershipStatus.Active;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Gets the global user identifier.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the membership's authorization eligibility.</summary>
    public MembershipStatus Status { get; private set; }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Gets the UTC modification timestamp.</summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>Gets the SQL Server concurrency token.</summary>
    public byte[] RowVersion { get; private set; } = [];

    /// <summary>Creates a membership between an existing tenant and user.</summary>
    /// <param name="tenantId">The existing tenant identifier.</param>
    /// <param name="userId">The existing global user identifier.</param>
    /// <param name="utcNow">The current UTC time.</param>
    /// <returns>A new active membership.</returns>
    /// <exception cref="ArgumentException">An identifier or timestamp is invalid.</exception>
    public static TenantMembership Create(Guid tenantId, Guid userId, DateTime utcNow)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty || utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Nonempty identifiers and UTC time are required.");
        }

        return new TenantMembership(tenantId, userId, utcNow);
    }

    /// <summary>Suspends authorization through this membership.</summary>
    /// <param name="utcNow">The current UTC time.</param>
    public void Suspend(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("UTC timestamp required.", nameof(utcNow));
        }

        Status = MembershipStatus.Suspended;
        UpdatedAtUtc = utcNow;
    }
}
