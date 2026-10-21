// <copyright file="TenantBootstrapProvisioner.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Domain.Tenants;
using EnterpriseIam.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIam.Identity.Infrastructure.Provisioning;

/// <summary>Provisions an organization and initial account under an internal transaction.</summary>
internal sealed class TenantBootstrapProvisioner : ITenantBootstrapProvisioner
{
    private readonly IamIdentityDbContext database;
    private readonly UserManager<ApplicationUser> users;
    private readonly TimeProvider time;

    /// <summary>Initializes a new instance of the <see cref="TenantBootstrapProvisioner"/> class.Constructs an internal provisioning operation.</summary>
    /// <param name="database">The SQL Server context.</param>
    /// <param name="users">The Identity framework account manager.</param>
    /// <param name="time">The time source for audit metadata.</param>
    public TenantBootstrapProvisioner(
        IamIdentityDbContext database,
        UserManager<ApplicationUser> users,
        TimeProvider time)
    {
        this.database = database;
        this.users = users;
        this.time = time;
    }

    /// <inheritdoc />
    public async Task<(Guid TenantId, Guid UserId)> CreateAsync(
        string tenantName,
        string tenantSlug,
        string userName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        // This internal service must only be called from trusted provisioning tooling.
        var now = this.time.GetUtcNow().UtcDateTime;
        var tenant = Tenant.Create(tenantName, tenantSlug, now);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await using var transaction = await this.database.Database.BeginTransactionAsync(cancellationToken);
        this.database.Tenants.Add(tenant);
        await this.database.SaveChangesAsync(cancellationToken);

        var identityResult = await this.users.CreateAsync(user, password);
        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException("Bootstrap account creation failed: " +
                string.Join("; ", identityResult.Errors.Select(error => error.Code)));
        }

        this.database.TenantMemberships.Add(TenantMembership.Create(tenant.Id, user.Id, now));
        await this.database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (tenant.Id, user.Id);
    }
}
