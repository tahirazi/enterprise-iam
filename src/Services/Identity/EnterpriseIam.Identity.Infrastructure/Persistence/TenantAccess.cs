using EnterpriseIam.Identity.Application.Abstractions;
using EnterpriseIam.Identity.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIam.Identity.Infrastructure.Persistence;

/// <summary>Checks active tenants, memberships, and global account status together.</summary>
internal sealed class TenantAccess : ITenantAccess
{
    private readonly IamIdentityDbContext _database;

    /// <summary>Creates a membership validator backed by relational data.</summary>
    /// <param name="database">The Identity persistence context.</param>
    public TenantAccess(IamIdentityDbContext database) => _database = database;

    /// <inheritdoc />
    public Task<bool> HasActiveMembershipAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || tenantId == Guid.Empty)
        {
            return Task.FromResult(false);
        }

        return _database.TenantMemberships.AsNoTracking()
            .Where(m => m.UserId == userId && m.TenantId == tenantId && m.Status == MembershipStatus.Active)
            .Join(_database.Tenants.AsNoTracking().Where(t => t.Status == TenantStatus.Active),
                m => m.TenantId, t => t.Id, (m, t) => m)
            .Join(_database.Users.AsNoTracking().Where(u => !u.IsDisabled),
                m => m.UserId, u => u.Id, (m, u) => m)
            .AnyAsync(cancellationToken);
    }
}
