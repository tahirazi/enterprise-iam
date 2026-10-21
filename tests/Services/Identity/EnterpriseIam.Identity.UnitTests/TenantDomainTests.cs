// <copyright file="TenantDomainTests.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Domain.Tenants;
using NUnit.Framework;

namespace EnterpriseIam.Identity.UnitTests;

[TestFixture]
public sealed class TenantDomainTests
{
    [Test]
    public void CreateTenant_WithInvalidSlug_Throws()
    {
        Assert.Throws<ArgumentException>(() => Tenant.Create("Acme", "ACME Corp", DateTime.UtcNow));
    }

    [Test]
    public void SuspendTenant_ChangesEligibility()
    {
        var tenant = Tenant.Create("Acme", "acme", DateTime.UtcNow);
        tenant.Suspend(DateTime.UtcNow);
        Assert.That(tenant.Status, Is.EqualTo(TenantStatus.Suspended));
    }

    [Test]
    public void CreateMembership_WithMissingIdentity_Throws()
    {
        Assert.Throws<ArgumentException>(() => TenantMembership.Create(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow));
    }
}
