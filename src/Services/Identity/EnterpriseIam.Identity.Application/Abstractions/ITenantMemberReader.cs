namespace EnterpriseIam.Identity.Application.Abstractions;

/// <summary>Restricts tenant-scoped membership reads to an authorized actor.</summary>
public interface ITenantMemberReader
{
    /// <summary>Returns members visible to a validated tenant member.</summary>
    /// <param name="actorUserId">The authenticated caller's global user identifier.</param>
    /// <param name="tenantId">The explicitly requested organization.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>Global user identifiers for members in the requested tenant.</returns>
    Task<IReadOnlyList<Guid>> GetMemberIdsAsync(Guid actorUserId, Guid tenantId, CancellationToken cancellationToken);
}
