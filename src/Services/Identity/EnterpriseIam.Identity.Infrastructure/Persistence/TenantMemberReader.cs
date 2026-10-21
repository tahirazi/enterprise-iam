using EnterpriseIam.Identity.Application.Abstractions;
using EnterpriseIam.Identity.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIam.Identity.Infrastructure.Persistence;

/// <summary>Provides tenant-isolated membership queries with mandatory actor verification.</summary>
internal sealed class TenantMemberReader : ITenantMemberReader
{
    private readonly IamIdentityDbContext _database;
    private readonly ITenantAccess _access;

    /// <summary>Initializes an authorized tenant-membership reader.</summary>
    /// <param name="database">The SQL Server data context.</param>
    /// <param name="access">The active membership authorization check.</param>
    public TenantMemberReader(IamIdentityDbContext database, ITenantAccess access)
    {
        _database = database;
        _access = access;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetMemberIdsAsync(
        Guid actorUserId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!await _access.HasActiveMembershipAsync(actorUserId, tenantId, cancellationToken))
        {
            throw new UnauthorizedAccessException("Tenant membership is not authorized.");
        }

        return await _database.TenantMemberships.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.Status == MembershipStatus.Active)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);
    }
}
